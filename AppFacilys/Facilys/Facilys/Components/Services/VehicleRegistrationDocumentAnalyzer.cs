using ElectronNET.API;
using ElectronNET.API.Entities;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Tesseract;

namespace Facilys.Components.Services
{
    public class VehicleRegistrationDocumentAnalyzer
    {
        private readonly VehicleRegistrationExtractor _extractor;
        private readonly OcrPerformanceMonitor _performanceMonitor;
        private readonly EnvironmentApp _envApp;
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;
        private readonly bool _useGeminiExtraction;
        byte[] data = Convert.FromBase64String("QUl6YVN5Q2R1OF9vamdjYmRxS2dLaWxkR29Nc3pMU2hGdTdPV2Vv");

        public VehicleRegistrationDocumentAnalyzer(EnvironmentApp envApp)
        {
            _extractor = new VehicleRegistrationExtractor();
            _performanceMonitor = new OcrPerformanceMonitor();
            _envApp = envApp;
            _httpClient = new HttpClient();
            _geminiApiKey = Encoding.UTF8.GetString(data); ;
            _useGeminiExtraction = !string.IsNullOrEmpty(_geminiApiKey);
        }

        public async Task<VehicleRegisterData> AnalyzeDocument(MemoryStream imageStream)
        {
            try
            {
                // Repositionner le curseur au début pour s'assurer que tout le contenu est lu
                imageStream.Position = 0;

                // Utiliser directement Tesseract avec le flux d'image
                string text = await PerformOcrAsync(imageStream);

                VehicleRegisterData data;

                if (_useGeminiExtraction)
                {
                    // Utilisation de Gemini pour l'extraction structurée
                    data = await ExtractWithGeminiAsync(text);

                    // Si Gemini échoue ou retourne des données incomplètes, utiliser l'extraction par regex comme fallback
                    if (data.ConfidenceScore < 50)
                    {
                        var regexData = _extractor.ExtractDocumentInfo(text);
                        data = CombineResults(data, regexData);
                    }
                }
                else
                {
                    // Extraire les informations du texte avec regex
                    data = _extractor.ExtractDocumentInfo(text);
                }

                _performanceMonitor.UpdateConfidenceMetrics(data);
                return _performanceMonitor.FindBestResult([data]);
            }
            catch (Exception ex)
            {
                // Logging des erreurs
                Console.WriteLine($"Erreur lors de l'analyse du document: {ex.Message}");
                return new VehicleRegisterData { Error = ex.Message };
            }
        }

        //private async Task<VehicleRegisterData> ExtractWithGeminiAsync(string ocrText)
        //{
        //    try
        //    {
        //        // Prompt optimisé pour structurer les données OCR de cartes grises françaises
        //        string prompt = $"Tu es un expert en analyse de cartes grises françaises. À partir de ce texte OCR brut, structure les données au format JSON strict. Texte OCR à analyser :" +
        //            $"{ocrText}" +
        //            $"Extrais et structure les champs suivants :" +
        //            $"-VIN(string) : Numéro d'identification du véhicule (17 caractères, souvent précédé de 'E')" +
        //            "- Registration(string) : Numéro d'immatriculation (format AA-123-AA, souvent précédé de 'A')" +
        //            "- ReleaseDate(string) : Date de première mise en circulation(format JJ/ MM / AAAA, souvent précédée de 'B')" +
        //            "-Name(string) : Nom du titulaire(souvent précédé de 'C.1' ou 'C1')" +
        //            "- Address(string) : Adresse complète(souvent précédée de 'C.3' ou 'C3')" +
        //            "-Mark(string) : Marque du véhicule(souvent précédée de 'D.1' ou 'D1')" +
        //            "- Model(string) : Modèle / Type commercial(souvent précédé de 'D.2' ou 'D2')" +
        //            "- SerieNumber(string) : Numéro de série" +
        //            "- Type(string) : Type, variante ou version(souvent précédé de 'J' ou 'J.1')" +
        //            "Instructions:" +
        //            "1.Analyse le texte OCR pour identifier les patterns typiques des cartes grises françaises" +
        //            "2.Si une information n'est pas trouvée ou est illisible, utilise null" +
        //            "3.Nettoie les espaces superflus et les caractères parasites" +
        //            "4.Formate les dates en JJ/ MM / AAAA" +
        //            "5.Pour l'immatriculation, formate en AA-123-AA" +
        //            "6.Retourne UNIQUEMENT le JSON sans commentaires" +
        //            "Exemple de format attendu:" +
        //            "{" +
        //            "'VIN':'VF1234567890123456'," +
        //            "'Registration': 'AB-123-CD'" +
        //            "'ReleaseDate': '01/01/2020'," +
        //            "'Name': 'DUPONT JEAN'," +
        //            "'Address': '123 RUE DE LA PAIX 75001 PARIS'," +
        //            "'Mark': 'RENAULT'," +
        //            "'Model': 'CLIO',+" +
        //            "'SerieNumber': 'ABC123'," +
        //            "'Type': 'BERLINE'" +
        //            "}";

        //        var request = new
        //        {
        //            contents = new[]
        //            {
        //                new
        //                {
        //                    parts = new[]
        //                    {
        //                        new { text = prompt }
        //                    }
        //                }
        //            }
        //        };

        //        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_geminiApiKey}";
        //        var response = await _httpClient.PostAsJsonAsync(url, request);
        //        response.EnsureSuccessStatusCode();

        //        var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>();
        //        string jsonText = jsonResponse?.candidates?[0]?.content?.parts?[0]?.text ?? "";

        //        // Nettoyage du JSON
        //        jsonText = ExtractJson(jsonText);

        //        var options = new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true,
        //            AllowTrailingCommas = true
        //        };

        //        var data = JsonSerializer.Deserialize<VehicleRegisterData>(jsonText, options) ?? new VehicleRegisterData();
        //        data.DataRead = ocrText; // Conserver le texte brut OCR

        //        return data;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Erreur lors de la structuration avec Gemini AI: {ex.Message}");
        //        // Fallback vers l'extraction par regex
        //        return _extractor.ExtractDocumentInfo(ocrText);
        //    }
        //}

        private async Task<VehicleRegisterData> ExtractWithGeminiAsync(string ocrText)
        {

            try
            {
                // 1. Construction du prompt optimisé
                var prompt = BuildGeminiPrompt(ocrText);

                // 2. Appel à l'API Gemini
                var jsonResponse = await CallGeminiApiAsync(prompt);

                // 3. Extraction et validation du JSON
                var result = ProcessGeminiResponse(jsonResponse, ocrText);

                // 4. Validation des données essentielles
                if (!string.IsNullOrEmpty(result.VIN)
                    && !string.IsNullOrEmpty(result.Registration))
                {
                    return result;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tentative 1 échouée {ex.Message}");
            }

            // Fallback vers l'extraction regex
            return _extractor.ExtractDocumentInfo(ocrText);
        }

        private string BuildGeminiPrompt(string ocrText)
        {
            var template = """
    Tu es un expert en analyse de cartes grises françaises. 
    À partir du texte OCR suivant, extrais les données structurées :

    Instructions :
    Format de sortie : Réponds uniquement avec un objet JSON strict, sans texte additionnel.
    Champs obligatoires :
        vin : Numéro de châssis (VIN)
        immatriculation : Numéro d’immatriculation (format AA-123-AA)
    Champs optionnels (si présents) :
        date_mise_en_circulation : Date de première mise en circulation (format JJ/MM/AAAA)
        nom proprietaire
        adresse
        marque
        modele
        energie
        puissance_fiscale
        genre
    Nettoyage des données :
        Supprime tous les espaces superflus dans les champs extraits.
        Corrige les erreurs OCR manifestes (par exemple : confusions entre O/0, I/1, etc.).

    **Exemple de sortie :**
    {
      "VIN": "VF7ABCD1234567890",
      "Registration": "AB-123-CD",
      "ReleaseDate": "12/06/2018",
      "Name": "DUPONT Jean",
      "Mark": "PEUGEOT",
      "Model": "208",
      "SerieNumber": "123456",
      "Address": "12 rue de Paris, 75000 Paris",
      "Type": "VP",
      "Error": ""
    }

    **Texte OCR à analyser :**
    {OCR_TEXT}
    """;

            return template.Replace("{OCR_TEXT}", ocrText.Trim());
        }

        private async Task<GeminiResponse> CallGeminiApiAsync(string prompt)
        {
            var request = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        },
                generationConfig = new
                {
                    temperature = 0.3, // Pour plus de précision
                    topP = 0.8
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_geminiApiKey}";

            using var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<GeminiResponse>();
        }

        private VehicleRegisterData ProcessGeminiResponse(GeminiResponse response, string ocrText)
        {
            if (response?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text is not string jsonText)
            {
                throw new InvalidOperationException("Réponse Gemini invalide");
            }

            // Extraction du JSON (gère les réponses avec commentaires)
            var cleanJson = ExtractJson(jsonText);

            // Désérialise dans un dictionnaire pour faire le mapping
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(cleanJson);

            var result = new VehicleRegisterData();

            // Mapping explicite des champs Gemini -> VehicleRegisterData
            if (dict.TryGetValue("vin", out var vin)) result.VIN = vin.GetString();
            if (dict.TryGetValue("immatriculation", out var reg)) result.Registration = reg.GetString();
            if (dict.TryGetValue("date_mise_en_circulation", out var date)) result.ReleaseDate = date.GetString();
            if (dict.TryGetValue("nom_proprietaire", out var name)) result.Name = name.GetString();
            if (dict.TryGetValue("marque", out var mark)) result.Mark = mark.GetString();
            if (dict.TryGetValue("modele", out var model)) result.Model = model.GetString();
            if (dict.TryGetValue("numero_de_serie", out var serie)) result.SerieNumber = serie.GetString();
            if (dict.TryGetValue("adresse", out var addr)) result.Address = addr.GetString();
            if (dict.TryGetValue("genre", out var type)) result.Type = type.GetString();

            // Si tu veux gérer d'autres champs, ajoute-les ici

            result.DataRead = ocrText;

            return result;
        }

        // Convertisseur custom pour les dates
        private class DateConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                var dateString = reader.GetString();
                if (DateTime.TryParseExact(dateString,
                    new[] { "dd/MM/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                {
                    return date.ToString("dd/MM/yyyy");
                }
                return null;
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
                => writer.WriteStringValue(value);
        }

        private static string ExtractJson(string input)
        {
            // Suppression des balises markdown si présentes
            input = input.Replace("```json", "").Replace("```", "");

            // Gestion des retours avec backticks
            int start = input.IndexOf('{');
            int end = input.LastIndexOf('}');

            if (start >= 0 && end > start)
            {
                return input[start..(end + 1)];
            }
            return input;
        }

        private VehicleRegisterData CombineResults(VehicleRegisterData geminiData, VehicleRegisterData regexData)
        {
            // Combine les résultats en privilégiant les données les plus fiables
            return new VehicleRegisterData
            {
                VIN = GetBestValue(geminiData.VIN, regexData.VIN, IsValidVin),
                Registration = GetBestValue(geminiData.Registration, regexData.Registration, IsValidRegistration),
                ReleaseDate = GetBestValue(geminiData.ReleaseDate, regexData.ReleaseDate, IsValidDate),
                Name = GetBestValue(geminiData.Name, regexData.Name),
                Address = GetBestValue(geminiData.Address, regexData.Address),
                Mark = GetBestValue(geminiData.Mark, regexData.Mark),
                Model = GetBestValue(geminiData.Model, regexData.Model),
                SerieNumber = GetBestValue(geminiData.SerieNumber, regexData.SerieNumber),
                Type = GetBestValue(geminiData.Type, regexData.Type),
                DataRead = geminiData.DataRead ?? regexData.DataRead
            };
        }

        private string GetBestValue(string value1, string value2, Func<string, bool> validator = null)
        {
            if (validator != null)
            {
                if (validator(value1)) return value1;
                if (validator(value2)) return value2;
            }

            if (!string.IsNullOrWhiteSpace(value1)) return value1;
            if (!string.IsNullOrWhiteSpace(value2)) return value2;
            return null;
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

        private static void ConfigureEngine(TesseractEngine engine)
        {
            engine.SetVariable("tessedit_pageseg_mode", "3"); // Mode de segmentation de page
            engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-/. àâäéèêëïîôöùûüÿç");
            engine.SetVariable("tessedit_ocr_engine_mode", "2"); // LSTM Only
            engine.SetVariable("user_defined_dpi", "300"); // Résolution optimale
            engine.SetVariable("debug_file", "/dev/null"); // Désactive les logs
        }

        private static string PostProcessText(string text)
        {
            return text.Replace("\n\n", " ")
                      .Replace("  ", " ")
                      .Trim();
        }

        private async Task<string> EnsureTessdataFiles()
        {
            string destDir;

            // Vérifier si on est en mode Electron (desktop)
            if (HybridSupport.IsElectronActive)
            {
                string pathDoc = await Electron.App.GetPathAsync(PathName.Documents);
                destDir = Path.Combine(pathDoc, _envApp.FolderData, "tessdata");
            }
            else
            {
                // Mode web - utiliser le stockage éphémère
                destDir = Path.Combine(Path.GetTempPath(), _envApp.FolderData, "tessdata");
            }

            // Vérifier si le répertoire existe et contient des fichiers
            bool needsCopy = !Directory.Exists(destDir) ||
                            !Directory.EnumerateFiles(destDir).Any();

            if (needsCopy)
            {
                Directory.CreateDirectory(destDir);
                var sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Tessdata");

                await CopyTessdataFiles(sourceDir, destDir);

                // En mode web, on peut aussi vérifier si les fichiers sont accessibles
                if (!HybridSupport.IsElectronActive)
                {
                    await VerifyTessdataAccess(destDir);
                }
            }

            return destDir;
        }

        private async Task CopyTessdataFiles(string sourceDir, string destDir)
        {
            try
            {
                foreach (var file in Directory.GetFiles(sourceDir))
                {
                    var destFile = Path.Combine(destDir, Path.GetFileName(file));
                    using var sourceStream = File.OpenRead(file);
                    using var destStream = File.Create(destFile);
                    await sourceStream.CopyToAsync(destStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur d'accès aux fichiers tessdata: {ex.Message}");
                throw;
            }
        }

        private async Task VerifyTessdataAccess(string dirPath)
        {
            try
            {
                // Vérifier qu'on peut bien lire les fichiers
                foreach (var file in Directory.GetFiles(dirPath))
                {
                    await File.ReadAllBytesAsync(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur d'accès aux fichiers tessdata: {ex.Message}");
                throw;
            }
        }

        // Méthodes de validation statiques
        private static bool IsValidVin(string vin) =>
            !string.IsNullOrEmpty(vin) &&
            vin.Length == 17 &&
            Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$");

        private static bool IsValidRegistration(string registration) =>
            !string.IsNullOrEmpty(registration) &&
            Regex.IsMatch(registration, @"^[A-Z]{2}-?\d{3}-?[A-Z]{2}$");

        private static bool IsValidDate(string date)
        {
            string[] formats = ["dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"];
            return DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }

        // Classes pour la désérialisation de la réponse Gemini
        private class GeminiResponse
        {
            public Candidate[] candidates { get; set; }
        }

        private class Candidate
        {
            public Content content { get; set; }
        }

        private class Content
        {
            public Part[] parts { get; set; }
        }

        private class Part
        {
            public string text { get; set; }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    public class VehicleRegistrationExtractor
    {
        private readonly List<FieldPattern> _fieldPatterns =
        [
            new("VIN", ["E"], @"E\s*([A-HJ-NPR-Z0-9]{17})",
                transform: v => v.Replace(" ", ""),
                validate: IsValidVin),

            new("Registration", ["A"], @"([A-Z]{2}[- ]?[0-9]{3}[- ]?[A-Z]{2})",
                transform: v => Regex.Replace(v, @"\s", "-")),

            new("ReleaseDate", ["B"], @"B\s*(\d{2}[/\-\.]\d{2}[/\-\.]\d{4})",
                validate: IsValidDate),

            new("Name", ["C.1", "C1"], @"C\.?1\s*([\p{L}\s-]+)(?=\s*C\.?2|\s*C\.?3)",
                multiLine: true,
                transform: CleanText),

            new("Address", ["C.3", "C3"], @"C\.?3\s*([\p{L}0-9\s,.-]+)(?=\s*C\.?4|D\.?1)",
                multiLine: true,
                transform: CleanText),

            new("Mark", ["D.1", "D1"], @"D\.?1\s*([\p{L}0-9\s-]+)(?=\s*D\.?2|\s*D\.?3)",
                transform: CleanText),

            new("Model", ["D.2", "D2"], @"D\.?2\s*([\p{L}0-9\s-]+)(?=\s*D\.?3|\s*E)",
                transform: CleanText),

            new("Type", ["J", "J.1", "J1"], @"J(?:\.1)?\s*([\p{L}0-9\s-]+)",
                transform: CleanText),

            new("SerieNumber", ["E"], @"E\s*([A-Z0-9]+)",
                transform: v => v.Replace(" ", ""))
        ];

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
            string[] formats = ["dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"];
            return DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }
    }

    public class OcrPerformanceMonitor
    {
        private readonly ConcurrentDictionary<string, int> _fieldConfidence = new();

        public VehicleRegisterData FindBestResult(List<VehicleRegisterData> results)
        {
            if (results.Count == 0)
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
        private readonly Dictionary<string, string> _data = [];

        public string VIN
        {
            get => _data.GetValueOrDefault(nameof(VIN));
            set => SetProperty(nameof(VIN), value);
        }

        public string Registration
        {
            get => _data.GetValueOrDefault(nameof(Registration));
            set => SetProperty(nameof(Registration), value);
        }

        public string ReleaseDate
        {
            get => _data.GetValueOrDefault(nameof(ReleaseDate));
            set => SetProperty(nameof(ReleaseDate), value);
        }

        public string Name
        {
            get => _data.GetValueOrDefault(nameof(Name));
            set => SetProperty(nameof(Name), value);
        }

        public string Mark
        {
            get => _data.GetValueOrDefault(nameof(Mark));
            set => SetProperty(nameof(Mark), value);
        }

        public string Model
        {
            get => _data.GetValueOrDefault(nameof(Model));
            set => SetProperty(nameof(Model), value);
        }

        public string SerieNumber
        {
            get => _data.GetValueOrDefault(nameof(SerieNumber));
            set => SetProperty(nameof(SerieNumber), value);
        }

        public string Address
        {
            get => _data.GetValueOrDefault(nameof(Address));
            set => SetProperty(nameof(Address), value);
        }

        public string Type
        {
            get => _data.GetValueOrDefault(nameof(Type));
            set => SetProperty(nameof(Type), value);
        }

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

        private static bool IsValidVin(string vin) =>
            !string.IsNullOrEmpty(vin) &&
            vin.Length == 17 &&
            Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$");

        private static bool IsValidRegistration(string registration) =>
            !string.IsNullOrEmpty(registration) &&
            Regex.IsMatch(registration, @"^[A-Z]{2}-?\d{3}-?[A-Z]{2}$");

        private static bool IsValidDate(string date)
        {
            string[] formats = ["dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy"];
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