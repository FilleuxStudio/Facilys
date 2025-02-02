using Facilys.Components.Constants;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using OpenCvSharp;

namespace Facilys.Components.Services
{
    public class VehicleRegistrationDocumentAnalyzer
    {
        private VehicleRegistrationExtractor _extractor;

        public VehicleRegistrationDocumentAnalyzer()
        {
            _extractor = new VehicleRegistrationExtractor();
        }

        public async Task CopyTessdataFiles()
        {
            var sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Tessdata");
            var destDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Facylis", "tessdata");

            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
        }

        //public async Task<VehicleRegisterData> AnalyzeDocument(MemoryStream imageStream)
        //{
        //    var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //    var directoryPathTessdata = Path.Combine(appDataPath, EnvironmentApp.FolderData, "tessdata");

        //    if (!Directory.Exists(directoryPathTessdata))
        //    {
        //        // Assurez-vous que le dossier existe
        //        Directory.CreateDirectory(directoryPathTessdata);
        //        await CopyTessdataFiles();
        //    }

        //    List<VehicleRegisterData> listData = new();
        //    int[] setting_pageseg = { 0, 3, 4, 6 };
        //    for (int i = 0; i < 4; i++)
        //    {
        //        // Utilisez le chemin absolu pour initialiser TesseractEngine
        //        using var engine = new TesseractEngine(directoryPathTessdata, "fra", EngineMode.Default);
        //        //Tu utilises le mode 3(PSM_AUTO), ce qui est une bonne idée pour les documents semi-structurés.Mais pour des cartes grises où les champs sont bien délimités, le mode 6(PSM_SINGLE_BLOCK) ou 4(PSM_SINGLE_COLUMN) peut donner de meilleurs résultats:
        //        //3 4 6
        //        engine.SetVariable("tessedit_pageseg_mode", setting_pageseg[i]); // PSM_SINGLE_BLOCK
        //        engine.SetVariable("load_system_dawg", "0"); // Désactive le dictionnaire par défaut
        //        engine.SetVariable("load_freq_dawg", "0"); // Désactive le dictionnaire de fréquence
        //        engine.SetVariable("user_words_suffix", "carte_grise.user-words"); // Ajoute un fichier de mots spécifiques

        //        using var img = Pix.LoadFromMemory(imageStream.ToArray());
        //        using var page = engine.Process(img);
        //        var text = page.GetText();
        //        listData.Add(ExtractDocumentInfo(text));
        //    }

        //    // Analyse du texte extrait
        //    return FindBestModel(listData);
        //}

        //public VehicleRegisterData ExtractDocumentInfo(string text)
        //{
        //    var data = new VehicleRegisterData();
        //    text = text.ToUpper().Replace(".", "");

        //    data.Registration = Regex.Match(text, @"\b[A-Z]{2}[-\s]?\d{3}[-\s]?[A-Z]{2}\b").Value;
        //    data.ReleaseDate = Regex.Match(text, @"\b\d{2}[/-]\d{2}[/-]\d{4}\b").Value;
        //    data.Name = Regex.Match(text, @"C\.?1\.?\s*(.*?)(?=\s*C\.?4|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
        //    data.Mark = Regex.Match(text, @"D\.?1\.?\s*(.*?)(?=\s*D\.?2|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
        //    data.Model = Regex.Match(text, @"D\.?3\.?\s*(.*?)(?=\s*D\.?4|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
        //    data.SerieNumber = Regex.Match(text, @"D\.?2\.?\s*(\S+)").Groups[1].Value.Trim();
        //    data.Address = Regex.Match(text, @"C\.?3\.?\s*(.*?)(?=\s*C\.?4|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
        //    data.Type = Regex.Match(text, @"J\.?\s*(\w+)").Groups[1].Value.Trim();
        //    data.VIN = Regex.Match(text, @"E\s*([A-HJ-NPR-Z0-9]{17})", RegexOptions.IgnoreCase).Groups[1].Value;

        //    return data;
        //}

        public async Task<VehicleRegisterData> AnalyzeDocument(MemoryStream imageStream)
        {

            try
            {
                var results = new List<VehicleRegisterData>();
                foreach (var pageSegMode in new[] { 3, 4, 6, 11 })
                {
                    var text = await PerformOcrAsync(imageStream.ToArray(), pageSegMode);
                    var data = _extractor.ExtractDocumentInfo(text);
                    results.Add(data);
                }
                return FindBestResult(results);

                //var processedImages = ImagePreprocessor.ProcessImage(imageStream.ToArray());
                //var results = new List<VehicleRegisterData>();

                //foreach (var image in processedImages)
                //{
                //    foreach (var pageSegMode in new[] { 3, 4, 6, 11 })
                //    {
                //        var text = await PerformOcrAsync(image, pageSegMode);
                //        var data = _extractor.ExtractDocumentInfo(text);
                //        results.Add(data);
                //    }
                //}
                //return FindBestResult(results);
            }
            catch (Exception ex)
            {
            }


            return new();
        }

        private async Task<string> PerformOcrAsync(byte[] imageData, int pageSegMode)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directoryPathTessdata = Path.Combine(appDataPath, EnvironmentApp.FolderData, "tessdata");

            if (!Directory.Exists(directoryPathTessdata))
            {
                // Assurez-vous que le dossier existe
                Directory.CreateDirectory(directoryPathTessdata);
                await CopyTessdataFiles();
            }

            using var engine = new TesseractEngine(directoryPathTessdata, "fra", EngineMode.LstmOnly);
            engine.SetVariable("tessedit_pageseg_mode", pageSegMode.ToString());
            engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-/. ");
            engine.SetVariable("tessedit_pageseg_mode", "1"); // Mode de segmentation automatique
            engine.SetVariable("tessedit_ocr_engine_mode", "2"); // Mode LSTM uniquement
            //engine.SetVariable("load_system_dawg", "0"); // Désactive le dictionnaire par défaut
            //engine.SetVariable("load_freq_dawg", "0"); // Désactive le dictionnaire de fréquence
            engine.SetVariable("user_words_suffix", "carte_grise.user-words"); // Ajoute un fichier de mots spécifiques

            using var img = Pix.LoadFromMemory(imageData);
            using var page = engine.Process(img);
            return page.GetText();
        }

        private VehicleRegisterData FindBestResult(List<VehicleRegisterData> results)
        {
            return results.OrderByDescending(r => r.CalculateConfidenceScore()).First();
        }

        //public static VehicleRegisterData FindBestModel(List<VehicleRegisterData> listData)
        //{
        //    // Trouver l'objet avec le score maximum
        //    return listData.OrderByDescending(data => data.CalculateScore()).First();
        //}
    }

    //public class VehicleRegisterData
    //{
    //    public string Registration { get; set; }
    //    public string Name { get; set; }
    //    public string Mark { get; set; }
    //    public string Model { get; set; }
    //    public string SerieNumber { get; set; }
    //    public string Address { get; set; }
    //    public string Type { get; set; }
    //    public string ReleaseDate { get; set; }
    //    public string VIN { get; set; }

    //    // Méthode pour calculer le score avec validation de cohérence
    //    public int CalculateScore()
    //    {
    //        int score = 0;

    //        // Vérification de la cohérence et ajout de points
    //        if (!string.IsNullOrEmpty(Registration) && IsValidRegistration(Registration)) score += 8;
    //        if (!string.IsNullOrEmpty(Name)) score += 3; 
    //        if (!string.IsNullOrEmpty(Mark)) score += 2;
    //        if (!string.IsNullOrEmpty(Model)) score += 2;
    //        if (!string.IsNullOrEmpty(SerieNumber)) score += 1; 
    //        if (!string.IsNullOrEmpty(Address)) score += 1;
    //        if (!string.IsNullOrEmpty(Type)) score += 1; 
    //        if (!string.IsNullOrEmpty(ReleaseDate) && IsValidDate(ReleaseDate)) score += 4; // Date valide
    //        if (!string.IsNullOrEmpty(VIN) && IsValidVIN(VIN)) score += 5; // VIN est critique

    //        return score;
    //    }

    //    // Vérifie si l'immatriculation est dans un format valide (ex: "AA-123-BB")
    //    private bool IsValidRegistration(string registration)
    //    {
    //        return Regex.IsMatch(registration, @"^[A-Z]{2}-\d{3}-[A-Z]{2}$");
    //    }

    //    // Vérifie si le VIN (Numéro d’identification du véhicule) est valide (17 caractères alphanumériques)
    //    private bool IsValidVIN(string vin)
    //    {
    //        return vin.Length == 17 && Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$"); // VIN n'inclut pas les I, O, Q
    //    }

    //    // Vérifie si une date est valide (format DD/MM/YYYY)
    //    private bool IsValidDate(string date)
    //    {
    //        return DateTime.TryParse(date, out _);
    //    }
    //}
    public class VehicleRegistrationExtractor
    {
        private readonly List<FieldPattern> _fieldPatterns;

        public VehicleRegistrationExtractor()
        {
            _fieldPatterns = new List<FieldPattern>
            {
                new FieldPattern("Registration",  new[] { "A"}, @"\b[A-Za-z]{2}-\d{3}-[A-Za-z]{2}\b",
    transform: v => v.Replace(" ", "-").Replace(".", "-")),

                  new FieldPattern("ReleaseDate", new[] { "B"},
                @"B\.?\s*(\d{2}/\d{2}/\d{4})", validate: IsValidDate),

              new FieldPattern("VIN", new[] { "E" },
    @"E\.?\s*([A-HJ-NPR-Z0-9]{17})",
    validate: IsValidVin,
    transform: v => v.Replace(" ", "").Replace(".", "").ToUpper()),

            

           new FieldPattern("Name", new[] { "C1" },
    @"C\.?1\s+([\w\s]+(?:\n[\w\s]+)?)",
    multiLine: true,
    transform: v => v.Replace("\n", " ").Trim()),

            new FieldPattern("Mark", new[] { "D1" },
                @"D\.?1\s+(\w+)"),

            new FieldPattern("Model", new[] { "D3" },
                @"D\.?3\s+([\w\s]+)"),

            new FieldPattern("Address", new[] { "C4.1", "C.4.1" },
                @"C4\.1\s*([\s\S]+?)(?=\n\s*\w|\Z)", multiLine: true),

            new FieldPattern("Type", new[] { "J1" },
                @"J1\s+(\w+)"),

          
            };
        }

        public VehicleRegisterData ExtractDocumentInfo(string text)
        {
            var data = new VehicleRegisterData();
            var lines = text.ToUpper().Replace(".", "").Split('\n').Select(l => l.Trim()).ToList();

            for (int i = 0; i < lines.Count; i++)
            {
                foreach (var pattern in _fieldPatterns)
                {
                    var (found, value, newIndex) = pattern.TryExtract(lines, i);
                    if (found)
                    {
                        data.SetProperty(pattern.FieldName, value);
                        if (newIndex > i)
                        {
                            i = newIndex - 1; // On soustrait 1 car la boucle for va incrémenter i
                            break; // Sortir de la boucle foreach pour passer à la ligne suivante
                        }
                    }
                }
            }
            data.Data = text;
            return data;
        }

        private static bool IsValidVin(string vin)
        {
            if (string.IsNullOrEmpty(vin) || (vin.Length >= 17 && vin.Length <= 18))
                return false;

            // Vérification des caractères valides
            return Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$");
        }


        private static bool IsValidDate(string date) =>
            DateTime.TryParseExact(date, new[] { "dd/MM/yyyy", "dd-MM-yyyy" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out _);
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
            Func<string, bool> validate = null, Func<string, string> transform = null,
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

                if (Triggers.Any(t => lines[currentIndex].Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    return ExtractValue(lines, currentIndex);
                }
            }

            return (false, null, startIndex);
        }

        private (bool, string, int) ExtractValue(List<string> lines, int triggerIndex)
        {
            var sb = new StringBuilder();
            int linesToCheck = MultiLine ? 5 : 1;

            for (int i = 0; i < linesToCheck; i++)
            {
                var currentLineIndex = triggerIndex + i;
                if (currentLineIndex >= lines.Count) break;

                var line = lines[currentLineIndex];
                var match = Pattern.Match(line);

                if (match.Success)
                {
                    var value = Transform(match.Groups[1].Value.Trim());
                    if (Validate(value))
                    {
                        return (true, value, currentLineIndex + 1);
                    }
                }

                if (MultiLine)
                {
                    sb.Append(line + " ");
                    if (line.Contains("  ") || line.Length == 0)
                    {
                        var fullValue = sb.ToString().Trim();
                        if (!string.IsNullOrEmpty(fullValue) && Validate(fullValue))
                            return (true, fullValue, currentLineIndex + 1);
                    }
                }
            }

            return (false, null, triggerIndex);
        }
    }

    public class VehicleRegisterData
    {
        private readonly Dictionary<string, string> _data = new();

        public string VIN => _data.GetValueOrDefault("VIN");
        public string Registration => _data.GetValueOrDefault("Registration");
        public string Name => _data.GetValueOrDefault("Name");
        public string Mark => _data.GetValueOrDefault("Mark");
        public string Model => _data.GetValueOrDefault("Model");
        public string SerieNumber => _data.GetValueOrDefault("SerieNumber");
        public string Address => _data.GetValueOrDefault("Address");
        public string Type => _data.GetValueOrDefault("Type");
        public string ReleaseDate => _data.GetValueOrDefault("ReleaseDate");
        public string Data {  get; set; } = string.Empty;

        public bool IsFieldSet(string fieldName) => _data.ContainsKey(fieldName);

        public void SetProperty(string fieldName, string value)
        {
            if (!string.IsNullOrEmpty(value) && !_data.ContainsKey(fieldName))
            {
                _data[fieldName] = value;
            }
        }

        public int CalculateConfidenceScore()
        {
            int score = 0;
            if (IsValidVin(VIN)) score += 30;
            if (IsValidRegistration(Registration)) score += 25;
            if (!string.IsNullOrEmpty(ReleaseDate)) score += 15;
            if (!string.IsNullOrEmpty(Address)) score += 10;
            if (!string.IsNullOrEmpty(Name)) score += 10;
            if (!string.IsNullOrEmpty(Mark)) score += 5;
            if (!string.IsNullOrEmpty(Model)) score += 5;
            return score;
        }

        private static bool IsValidVin(string vin) =>
            !string.IsNullOrEmpty(vin) && vin.Length == 17;

        private static bool IsValidRegistration(string registration) =>
            !string.IsNullOrEmpty(registration) && registration.Length >= 7;
    }

    public static class ImagePreprocessor
    {
        public static List<byte[]> ProcessImage(byte[] imageBytes)
        {
            var processedImages = new List<byte[]>();

            using (var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color))
            {
                var rotated = AutoRotate(mat);

                processedImages.Add(ProcessVariant(rotated, ThresholdTypes.Binary));
                //processedImages.Add(ProcessVariant(rotated, ThresholdTypes.Adaptive));
                processedImages.Add(ProcessVariant(rotated.MedianBlur(3), ThresholdTypes.Binary));
                processedImages.Add(ProcessVariant(rotated.GaussianBlur(new Size(3, 3), 0), ThresholdTypes.Otsu));
            }

            return processedImages;
        }

        private static byte[] ProcessVariant(Mat input, ThresholdTypes thresholdType)
        {
            using var gray = input.CvtColor(ColorConversionCodes.BGR2GRAY);
            using var thresholded = new Mat();
            Cv2.Threshold(gray, thresholded, 0, 255, thresholdType);
            return thresholded.ImEncode(".png");
        }

        private static Mat AutoRotate(Mat input)
        {
            using var edges = new Mat();
            Cv2.Canny(input, edges, 50, 200);
            var lines = Cv2.HoughLinesP(edges, 1, Math.PI / 180, 50, 50, 10);

            if (lines.Length > 0)
            {
                var angles = lines.Select(l => Math.Atan2(l.P2.Y - l.P1.Y, l.P2.X - l.P1.X))
                                 .Where(a => Math.Abs(a) > 0.1)
                                 .DefaultIfEmpty(0)
                                 .Average();

                if (Math.Abs(angles) > 0.1)
                {
                    var center = new Point2f(input.Width / 2f, input.Height / 2f);
                    var rotationMatrix = Cv2.GetRotationMatrix2D(center, angles * 180 / Math.PI, 1.0);
                    Cv2.WarpAffine(input, input, rotationMatrix, input.Size());
                }
            }
            return input;
        }
    }
}
