using Facilys.Components.Constants;
using Microsoft.AspNetCore.Components.Forms;
using OpenCvSharp;
using System.Text.RegularExpressions;
using Tesseract;

namespace Facilys.Components.Services
{
    public class VehicleRegistrationDocumentAnalyzer
    {
        public async Task<VehicleRegisterData> AnalyzeDocument(MemoryStream imageStream)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var directoryPathTessdata = Path.Combine(appDataPath, EnvironmentApp.FolderData, "tessdata");
            // Assurez-vous que le dossier existe
            Directory.CreateDirectory(directoryPathTessdata);

            // Utilisez le chemin absolu pour initialiser TesseractEngine
            using var engine = new TesseractEngine(directoryPathTessdata, "fra", EngineMode.Default);
            //Tu utilises le mode 3(PSM_AUTO), ce qui est une bonne idée pour les documents semi-structurés.Mais pour des cartes grises où les champs sont bien délimités, le mode 6(PSM_SINGLE_BLOCK) ou 4(PSM_SINGLE_COLUMN) peut donner de meilleurs résultats:
            //3 4 6
            engine.SetVariable("tessedit_pageseg_mode", 0); // PSM_SINGLE_BLOCK
            //engine.SetVariable("tessedit_pageseg_mode", "3"); // PSM_AUTO[5]
            engine.SetVariable("load_system_dawg", "0"); // Désactive le dictionnaire par défaut
            engine.SetVariable("load_freq_dawg", "0"); // Désactive le dictionnaire de fréquence
            engine.SetVariable("user_words_suffix", "carte_grise.user-words"); // Ajoute un fichier de mots spécifiques

            using var img = Pix.LoadFromMemory(imageStream.ToArray());
            using var page = engine.Process(img);
            var text = page.GetText();

            // Analyse du texte extrait
            return ExtractDocumentInfo(text);
        }


        public VehicleRegisterData ExtractDocumentInfo(string text)
        {
            var info = new VehicleRegisterData();

            // Extraction de l'immatriculation
            info.Immatriculation = ExtractImmatriculation(text);

            // Extraction du nom du propriétaire
            info.Nom = ExtractOwnerName(text);

            // Extraction de la marque
            info.Marque = ExtractMarque(text);

            // Extraction du modèle
            info.Modele = ExtractModele(text);

            // Extraction du numéro de série
            info.NumeroSerie = ExtractNumeroSerie(text);

            // Extraction de la date de mise en circulation
            info.DateMiseEnCirculation = ExtractDateMiseEnCirculation(text);

            return info;
        }

        private string ExtractImmatriculation(string text)
        {
            var match = Regex.Match(text, @"[A-Z]{2}-\d{3}-[A-Z]{2}");
            return match.Success ? match.Value : string.Empty;
        }

        private string ExtractOwnerName(string text)
        {
            // Supposons que le nom est précédé de "NOM :" dans le texte
            var match = Regex.Match(text, @"NOM\s*:\s*([A-Z\s-]+)");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string ExtractMarque(string text)
        {
            // Supposons que la marque est précédée de "MARQUE :" dans le texte
            var match = Regex.Match(text, @"MARQUE\s*:\s*([A-Za-z0-9\s-]+)");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string ExtractModele(string text)
        {
            // Supposons que le modèle est précédé de "MODELE :" dans le texte
            var match = Regex.Match(text, @"MODELE\s*:\s*([A-Za-z0-9\s-]+)");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string ExtractNumeroSerie(string text)
        {
            // Supposons que le numéro de série est précédé de "N° SERIE :" dans le texte
            var match = Regex.Match(text, @"N° SERIE\s*:\s*([A-Za-z0-9]+)");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string ExtractDateMiseEnCirculation(string text)
        {
            // Supposons que la date est au format "JJ/MM/AAAA" et précédée de "DATE MISE EN CIRCULATION :"
            var match = Regex.Match(text, @"DATE MISE EN CIRCULATION\s*:\s*(\d{2}/\d{2}/\d{4})");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
    }

    public class VehicleRegisterData
    {
        public string Immatriculation { get; set; }
        public string Nom { get; set; }
        public string Marque { get; set; }
        public string Modele { get; set; }
        public string NumeroSerie { get; set; }
        public string DateMiseEnCirculation { get; set; }
        // Ajoutez d'autres propriétés selon les champs de la carte grise
    }
}
