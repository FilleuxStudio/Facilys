using Facilys.Components.Constants;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;

namespace Facilys.Components.Services
{
    public class VehicleRegistrationDocumentAnalyzer
    {
        private readonly VehicleRegistrationExtractor _extractor;
        private readonly OcrPerformanceMonitor _performanceMonitor;

        public VehicleRegistrationDocumentAnalyzer()
        {
            _extractor = new VehicleRegistrationExtractor();
            _performanceMonitor = new OcrPerformanceMonitor();
        }

        public async Task<VehicleRegisterData> AnalyzeDocument(MemoryStream imageStream)
        {
            try
            {
                // Repositionner le curseur au début pour s'assurer que tout le contenu est lu
                imageStream.Position = 0;

                // Utiliser directement Tesseract avec le flux d'image
                string text = await PerformOcrAsync(imageStream);

                // Extraire les informations du texte
                var data = _extractor.ExtractDocumentInfo(text);
                _performanceMonitor.UpdateConfidenceMetrics(data);

                return _performanceMonitor.FindBestResult(new List<VehicleRegisterData> { data });
            }
            catch (Exception ex)
            {
                // Logging des erreurs
                Console.WriteLine($"Erreur lors de l'analyse du document: {ex.Message}");
                return new VehicleRegisterData { Error = ex.Message };
            }
        }

        private async Task<string> PerformOcrAsync(MemoryStream imageStream)
        {
            var tessdataPath = await EnsureTessdataFiles();

            using var engine = new TesseractEngine(tessdataPath, "fra", EngineMode.LstmOnly);
            ConfigureEngine(engine);

            // Lire l'image directement à partir du MemoryStream
            byte[] imageData = imageStream.ToArray();
            using var img = Pix.LoadFromMemory(imageData);
            using var page = engine.Process(img);

            return PostProcessText(page.GetText());
        }

        private void ConfigureEngine(TesseractEngine engine)
        {
            engine.SetVariable("tessedit_pageseg_mode", "3"); // Mode de segmentation de page
            engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-/. ");
            engine.SetVariable("tessedit_ocr_engine_mode", "2"); // LSTM Only
            engine.SetVariable("user_defined_dpi", "300"); // Résolution optimale
            engine.SetVariable("debug_file", "/dev/null"); // Désactive les logs
        }

        private string PostProcessText(string text)
        {
            return text.Replace("\n\n", " ")
                      .Replace("  ", " ")
                      .Trim();
        }

        private async Task<string> EnsureTessdataFiles()
        {
            var destDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                EnvironmentApp.FolderData,
                "tessdata");

            if (!Directory.Exists(destDir) || !Directory.GetFiles(destDir).Any())
            {
                Directory.CreateDirectory(destDir);
                var sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Tessdata");
                await CopyTessdataFiles(sourceDir, destDir);
            }
            return destDir;
        }

        private async Task CopyTessdataFiles(string sourceDir, string destDir)
        {
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                using var sourceStream = File.OpenRead(file);
                using var destStream = File.Create(Path.Combine(destDir, Path.GetFileName(file)));
                await sourceStream.CopyToAsync(destStream);
            }
        }
    }

    public class VehicleRegistrationExtractor
    {
        private readonly List<FieldPattern> _fieldPatterns = new()
        {
            new("VIN", new[] { "E" }, @"E\s*([A-HJ-NPR-Z0-9]{17})",
                transform: v => v.Replace(" ", ""),
                validate: IsValidVin),

            new("Registration", new[] { "A" }, @"([A-Z]{2}[- ]?[0-9]{3}[- ]?[A-Z]{2})",
                transform: v => Regex.Replace(v, @"\s", "-")),

            new("ReleaseDate", new[] { "B" }, @"B\s*(\d{2}[/\-\.]\d{2}[/\-\.]\d{4})",
                validate: IsValidDate),

            new("Name", new[] { "C.1", "C1" }, @"C\.?1\s*([\p{L}\s-]+)(?=\s*C\.?2|\s*C\.?3)",
                multiLine: true,
                transform: CleanText),

            new("Address", new[] { "C.3", "C3" }, @"C\.?3\s*([\p{L}0-9\s,.-]+)(?=\s*C\.?4|D\.?1)",
                multiLine: true,
                transform: CleanText),

            new("Mark", new[] { "D.1", "D1" }, @"D\.?1\s*([\p{L}0-9\s-]+)(?=\s*D\.?2|\s*D\.?3)",
                transform: CleanText),

            new("Model", new[] { "D.2", "D2" }, @"D\.?2\s*([\p{L}0-9\s-]+)(?=\s*D\.?3|\s*E)",
                transform: CleanText),

            new("Type", new[] { "J", "J.1", "J1" }, @"J(?:\.1)?\s*([\p{L}0-9\s-]+)",
                transform: CleanText),

            new("SerieNumber", new[] { "E" }, @"E\s*([A-Z0-9]+)",
                transform: v => v.Replace(" ", ""))
        };

        public VehicleRegisterData ExtractDocumentInfo(string text)
        {
            var data = new VehicleRegisterData();
            var lines = text.Split('\n').Select(l => l.Trim()).ToList();

            data.DataRead = text;

            for (int i = 0; i < lines.Count; i++)
            {
                foreach (var pattern in _fieldPatterns)
                {
                    var (found, value, newIndex) = pattern.TryExtract(lines, i);
                    if (found)
                    {
                        data.SetProperty(pattern.FieldName, value);
                        i = newIndex - 1;
                        break;
                    }
                }
            }
            return data;
        }

        private static string CleanText(string input) =>
            Regex.Replace(input, @"\s+", " ").Trim();

        private static bool IsValidVin(string vin) =>
            vin.Length == 17 && Regex.IsMatch(vin, "^[A-HJ-NPR-Z0-9]{17}$");

        private static bool IsValidDate(string date)
        {
            string[] formats = { "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy" };
            return DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }
    }

    public class OcrPerformanceMonitor
    {
        private readonly ConcurrentDictionary<string, int> _fieldConfidence = new();

        public VehicleRegisterData FindBestResult(List<VehicleRegisterData> results)
        {
            if (!results.Any())
                return new VehicleRegisterData();

            return results.OrderByDescending(r => r.ConfidenceScore)
                          .FirstOrDefault() ?? new VehicleRegisterData();
        }

        public void UpdateConfidenceMetrics(VehicleRegisterData data)
        {
            foreach (var prop in data.GetProperties())
            {
                if (!string.IsNullOrEmpty(prop.Value))
                {
                    _fieldConfidence.AddOrUpdate(prop.Key, 1, (_, v) => v + 1);
                }
            }
        }
    }

    public class VehicleRegisterData
    {
        private readonly Dictionary<string, string> _data = new();

        public string VIN => _data.GetValueOrDefault(nameof(VIN));
        public string Registration => _data.GetValueOrDefault(nameof(Registration));
        public string ReleaseDate => _data.GetValueOrDefault(nameof(ReleaseDate));
        public string Name => _data.GetValueOrDefault(nameof(Name));
        public string Mark => _data.GetValueOrDefault(nameof(Mark));
        public string Model => _data.GetValueOrDefault(nameof(Model));
        public string SerieNumber => _data.GetValueOrDefault(nameof(SerieNumber));
        public string Address => _data.GetValueOrDefault(nameof(Address));
        public string Type => _data.GetValueOrDefault(nameof(Type));
        public string Error { get; set; }
        public string DataRead { get; set; }
        public int ConfidenceScore => CalculateConfidenceScore();

        public void SetProperty(string fieldName, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                _data[fieldName] = value.Trim();
        }

        public IEnumerable<KeyValuePair<string, string>> GetProperties()
        {
            return _data.Where(kv => !string.IsNullOrEmpty(kv.Value));
        }

        private int CalculateConfidenceScore()
        {
            int score = 0;
            if (IsValidVin(VIN)) score += 30;
            if (IsValidRegistration(Registration)) score += 25;
            if (IsValidDate(ReleaseDate)) score += 20;
            if (!string.IsNullOrEmpty(Name)) score += 5;
            if (!string.IsNullOrEmpty(Address)) score += 5;
            if (!string.IsNullOrEmpty(Mark)) score += 5;
            if (!string.IsNullOrEmpty(Model)) score += 5;
            if (!string.IsNullOrEmpty(Type)) score += 5;

            return Math.Min(100, score);
        }

        private bool IsValidVin(string vin) =>
            !string.IsNullOrEmpty(vin) &&
            vin.Length == 17 &&
            Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$");

        private bool IsValidRegistration(string registration) =>
            !string.IsNullOrEmpty(registration) &&
            Regex.IsMatch(registration, @"^[A-Z]{2}-?\d{3}-?[A-Z]{2}$");

        private bool IsValidDate(string date)
        {
            string[] formats = { "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy" };
            return DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }
    }

    public class FieldPattern
    {
        public string FieldName { get; }
        public string[] Triggers { get; }
        public Regex Pattern { get; }
        public Func<string, bool> Validate { get; }
        public Func<string, string> Transform { get; }
        public bool MultiLine { get; }

        public FieldPattern(string fieldName, string[] triggers, string regexPattern,
            Func<string, bool> validate = null,
            Func<string, string> transform = null,
            bool multiLine = false)
        {
            FieldName = fieldName;
            Triggers = triggers;
            Pattern = new Regex(regexPattern, RegexOptions.IgnoreCase);
            Validate = validate ?? (_ => true);
            Transform = transform ?? (v => v);
            MultiLine = multiLine;
        }

        public (bool found, string value, int newIndex) TryExtract(List<string> lines, int startIndex)
        {
            for (int offset = 0; offset <= 2; offset++)
            {
                var currentIndex = startIndex + offset;
                if (currentIndex >= lines.Count) break;

                foreach (var trigger in Triggers)
                {
                    if (lines[currentIndex].Contains(trigger, StringComparison.OrdinalIgnoreCase))
                    {
                        return ExtractValue(lines, currentIndex);
                    }
                }
            }
            return (false, null, startIndex);
        }

        private (bool, string, int) ExtractValue(List<string> lines, int triggerIndex)
        {
            var sb = new StringBuilder();
            int linesToCheck = MultiLine ? 3 : 1;

            for (int i = 0; i < linesToCheck; i++)
            {
                var currentLineIndex = triggerIndex + i;
                if (currentLineIndex >= lines.Count) break;

                var line = lines[currentLineIndex];
                var match = Pattern.Match(line);

                if (match.Success)
                {
                    var rawValue = match.Groups[1].Value.Trim();
                    var transformed = Transform(rawValue);
                    if (Validate(transformed))
                    {
                        return (true, transformed, currentLineIndex + 1);
                    }
                }

                if (MultiLine)
                {
                    sb.Append(line + " ");
                    var fullText = sb.ToString();
                    var multiLineMatch = Pattern.Match(fullText);

                    if (multiLineMatch.Success)
                    {
                        var rawValue = multiLineMatch.Groups[1].Value.Trim();
                        var transformed = Transform(rawValue);
                        if (Validate(transformed))
                        {
                            return (true, transformed, currentLineIndex + 1);
                        }
                    }

                    if (line.Contains("  ") || line.Length == 0)
                    {
                        var fullValue = sb.ToString().Trim();
                        if (Validate(fullValue))
                        {
                            return (true, fullValue, currentLineIndex + 1);
                        }
                    }
                }
            }
            return (false, null, triggerIndex);
        }
    }
}
