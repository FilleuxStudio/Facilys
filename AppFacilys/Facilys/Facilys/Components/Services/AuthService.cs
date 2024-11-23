using ElectronNET.API.Entities;
using ElectronNET.API;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using Facilys.Components.Constants;

namespace Facilys.Components.Services
{
    public class AuthService
    {
        private const string CookieFileName = "cookieConnect.json";
        private const int CookieValidityDays = 30;

        /// <summary>
        /// Vérifie si l'utilisateur est authentifié en chargeant et validant les données du cookie.
        /// Utilise : System.Text.Json pour la désérialisation.
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var cookieData = await LoadCookieDataAsync();
            if (cookieData == null) return false;

            return cookieData.IsConnected &&
                   DateTime.UtcNow < cookieData.ExpirationDate;
        }

        /// <summary>
        /// Authentifie l'utilisateur en créant et sauvegardant un nouveau cookie.
        /// Utilise : System.Security.Cryptography pour générer la clé SHA256.
        /// </summary>
        public async Task SetAuthenticatedAsync(string email, string password)
        {
            string username = "test";

            bool flag = APIWebSiteService.PostConnectionUser(email, password);

            var cookieData = new CookieData
            {
                Username = username,
                Email = email,
                IsConnected = true,
                ExpirationDate = DateTime.UtcNow.AddDays(CookieValidityDays),
                Key = GenerateSha256(username + DateTime.UtcNow.Ticks)
            };

            await SaveCookieDataAsync(cookieData);
        }

        /// <summary>
        /// Déconnecte l'utilisateur en supprimant les données du cookie.
        /// </summary>
        public async Task LogoutAsync()
        {
            await SaveCookieDataAsync(null);
        }

        /// <summary>
        /// Charge les données du cookie depuis le fichier JSON.
        /// Utilise : ElectronNET.API pour accéder au chemin AppData,
        /// System.Text.Json pour la désérialisation.
        /// </summary>
        private async Task<CookieData> LoadCookieDataAsync()
        {
            var appDataPath = await Electron.App.GetPathAsync(PathName.Documents);

            var filePath = Path.Combine(appDataPath, EnvironmentApp.FolderData, CookieFileName);

            if (!File.Exists(filePath)) return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<CookieData>(json);
        }

        /// <summary>
        /// Sauvegarde les données du cookie dans un fichier JSON.
        /// Utilise : ElectronNET.API pour accéder au chemin AppData,
        /// System.Text.Json pour la sérialisation.
        /// </summary>
        private async Task SaveCookieDataAsync(CookieData cookieData)
        {
            // Récupère le chemin "Documents"
            var appDataPath = await Electron.App.GetPathAsync(PathName.Documents);

            // Combine le chemin pour le dossier de l'application et le fichier cookie
            var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
            var filePath = Path.Combine(directoryPath, CookieFileName);

            try
            {
                // Assure que le répertoire existe
                Directory.CreateDirectory(directoryPath);

                if (cookieData == null)
                {
                    // Supprime le fichier s'il existe
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    return;
                }

                // Sérialise les données du cookie en JSON
                var json = JsonSerializer.Serialize(cookieData, new JsonSerializerOptions
                {
                    WriteIndented = true // Facilite la lecture du fichier
                });

                // Écrit le JSON dans le fichier
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                // Gère les exceptions éventuelles (par exemple, accès refusé)
                Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Génère un hash SHA256 à partir d'une chaîne d'entrée.
        /// Utilise : System.Security.Cryptography pour le hachage SHA256,
        /// System.Text.Encoding pour la conversion de chaîne en bytes.
        /// </summary>
        private string GenerateSha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }

        public static async Task EnsureApplicationFolderExistsAsync()
        {
            // Obtient le chemin des documents utilisateur via Electron
            var documentsPath = await Electron.App.GetPathAsync(PathName.Documents);

            // Combine le chemin pour inclure le dossier "Facilys"
            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");

            // Vérifie si le dossier existe, sinon le crée
            if (!Directory.Exists(facilysFolderPath))
            {
                Directory.CreateDirectory(facilysFolderPath);
            }
        }
    }

    public class CookieData
    {
        public string Username { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public bool IsConnected { get; set; } = false;
        public DateTime ExpirationDate { get; set; } = DateTime.Now;
        public string Key { get; set; } = String.Empty;
    }
}