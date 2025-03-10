using System.Text.Json;

namespace Facilys.Components.Services
{
    public class SaveDocuments
    {
        /// <summary>
        /// Méthode pour enregistrer un fichier pdf
        /// </summary>
        /// <param name="filePath">chemin</param>
        /// <param name="fileName">nom du fichier</param>
        /// <param name="pdfBytes">tableau byte</param>
        /// <returns>bool</returns>
        public static async Task<bool> SaveDocumentsPDF(string filePath, string fileName, byte[] pdfBytes)
        {
            try
            {
                string fullPath = Path.Combine(filePath, fileName);

                // Crée le dossier s'il n'existe pas
                Directory.CreateDirectory(filePath);

                // Écrit directement le tableau d'octets dans un fichier
                await File.WriteAllBytesAsync(fullPath, pdfBytes);

                return true;
            }
            catch (Exception ex)
            {
                // Log l'exception ou gérez-la selon vos besoins
                Console.WriteLine($"Erreur lors de la sauvegarde du PDF : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Méthode pour enregistrer un fichier sérializer json
        /// </summary>
        /// <typeparam name="T">objet</typeparam>
        /// <param name="filePath">chemin</param>
        /// <param name="fileName">nom du fichier</param>
        /// <param name="objectToSerialize">objet</param>
        /// <returns>bool</returns>
        public static async Task<bool> SaveSerializedObjects<T>(string filePath, string fileName, T objectToSerialize)
        {
            try
            {
                string fullPath = Path.Combine(filePath, fileName);

                // Crée le dossier s'il n'existe pas
                Directory.CreateDirectory(filePath);

                // Sérialise l'objet en JSON
                string jsonString = JsonSerializer.Serialize(objectToSerialize, new JsonSerializerOptions
                {
                    WriteIndented = true // Pour un JSON formaté et lisible
                });

                // Écrit le JSON dans un fichier
                await File.WriteAllTextAsync(fullPath, jsonString);

                return true;
            }
            catch (Exception ex)
            {
                // Log l'exception ou gérez-la selon vos besoins
                Console.WriteLine($"Erreur lors de la sauvegarde du fichier sérialisé : {ex.Message}");
                return false;
            }
        }

    }
}
