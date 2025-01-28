using Facilys.Components.Constants;
using Microsoft.AspNetCore.Components.Forms;
using OpenCvSharp;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Tesseract;

namespace Facilys.Components.Services
{
    public class VehicleRegistrationDocumentAnalyzer
    {

        public async Task CopyTessdataFiles()
        {
            var sourceDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Tessdata");
            var destDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"Facylis", "tessdata");

            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                var destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }
        }

        public async Task<VehicleRegisterData> AnalyzeDocument(MemoryStream imageStream)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directoryPathTessdata = Path.Combine(appDataPath, EnvironmentApp.FolderData, "tessdata");

            if (!Directory.Exists(directoryPathTessdata))
            {
                // Assurez-vous que le dossier existe
                Directory.CreateDirectory(directoryPathTessdata);
                await CopyTessdataFiles();
            }

            List<VehicleRegisterData> listData = new();
            int[] setting_pageseg = { 0, 3, 4, 6 };
            for (int i = 0; i < 4; i++)
            {
                // Utilisez le chemin absolu pour initialiser TesseractEngine
                using var engine = new TesseractEngine(directoryPathTessdata, "fra", EngineMode.Default);
                //Tu utilises le mode 3(PSM_AUTO), ce qui est une bonne idée pour les documents semi-structurés.Mais pour des cartes grises où les champs sont bien délimités, le mode 6(PSM_SINGLE_BLOCK) ou 4(PSM_SINGLE_COLUMN) peut donner de meilleurs résultats:
                //3 4 6
                engine.SetVariable("tessedit_pageseg_mode", setting_pageseg[i]); // PSM_SINGLE_BLOCK
                engine.SetVariable("load_system_dawg", "0"); // Désactive le dictionnaire par défaut
                engine.SetVariable("load_freq_dawg", "0"); // Désactive le dictionnaire de fréquence
                engine.SetVariable("user_words_suffix", "carte_grise.user-words"); // Ajoute un fichier de mots spécifiques

                using var img = Pix.LoadFromMemory(imageStream.ToArray());
                using var page = engine.Process(img);
                var text = page.GetText();
                listData.Add(ExtractDocumentInfo(text));
            }

            // Analyse du texte extrait
            return FindBestModel(listData);
        }

        public VehicleRegisterData ExtractDocumentInfo(string text)
        {
            var data = new VehicleRegisterData();
            text = text.ToUpper().Replace(".", "");

            data.Registration = Regex.Match(text, @"\b[A-Z]{2}[-\s]?\d{3}[-\s]?[A-Z]{2}\b").Value;
            data.ReleaseDate = Regex.Match(text, @"\b\d{2}[/-]\d{2}[/-]\d{4}\b").Value;
            data.Name = Regex.Match(text, @"C\.?1\.?\s*(.*?)(?=\s*C\.?4|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
            data.Mark = Regex.Match(text, @"D\.?1\.?\s*(.*?)(?=\s*D\.?2|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
            data.Model = Regex.Match(text, @"D\.?3\.?\s*(.*?)(?=\s*D\.?4|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
            data.SerieNumber = Regex.Match(text, @"D\.?2\.?\s*(\S+)").Groups[1].Value.Trim();
            data.Address = Regex.Match(text, @"C\.?3\.?\s*(.*?)(?=\s*C\.?4|$)", RegexOptions.Singleline).Groups[1].Value.Trim();
            data.Type = Regex.Match(text, @"J\.?\s*(\w+)").Groups[1].Value.Trim();
            data.VIN = Regex.Match(text, @"E\s*([A-HJ-NPR-Z0-9]{17})", RegexOptions.IgnoreCase).Groups[1].Value;

            return data;
        }


        public static VehicleRegisterData FindBestModel(List<VehicleRegisterData> listData)
        {
            // Trouver l'objet avec le score maximum
            return listData.OrderByDescending(data => data.CalculateScore()).First();
        }
    }

    public class VehicleRegisterData
    {
        public string Registration { get; set; }
        public string Name { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public string SerieNumber { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public string ReleaseDate { get; set; }
        public string VIN { get; set; }

        // Méthode pour calculer le score avec validation de cohérence
        public int CalculateScore()
        {
            int score = 0;

            // Vérification de la cohérence et ajout de points
            if (!string.IsNullOrEmpty(Registration) && IsValidRegistration(Registration)) score += 8;
            if (!string.IsNullOrEmpty(Name)) score += 3; 
            if (!string.IsNullOrEmpty(Mark)) score += 2;
            if (!string.IsNullOrEmpty(Model)) score += 2;
            if (!string.IsNullOrEmpty(SerieNumber)) score += 1; 
            if (!string.IsNullOrEmpty(Address)) score += 1;
            if (!string.IsNullOrEmpty(Type)) score += 1; 
            if (!string.IsNullOrEmpty(ReleaseDate) && IsValidDate(ReleaseDate)) score += 4; // Date valide
            if (!string.IsNullOrEmpty(VIN) && IsValidVIN(VIN)) score += 5; // VIN est critique

            return score;
        }

        // Vérifie si l'immatriculation est dans un format valide (ex: "AA-123-BB")
        private bool IsValidRegistration(string registration)
        {
            return Regex.IsMatch(registration, @"^[A-Z]{2}-\d{3}-[A-Z]{2}$");
        }

        // Vérifie si le VIN (Numéro d’identification du véhicule) est valide (17 caractères alphanumériques)
        private bool IsValidVIN(string vin)
        {
            return vin.Length == 17 && Regex.IsMatch(vin, @"^[A-HJ-NPR-Z0-9]{17}$"); // VIN n'inclut pas les I, O, Q
        }

        // Vérifie si une date est valide (format DD/MM/YYYY)
        private bool IsValidDate(string date)
        {
            return DateTime.TryParse(date, out _);
        }
    }
}
