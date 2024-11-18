using ElectronNET.API.Entities;
using ElectronNET.API;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

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
        public async Task SetAuthenticatedAsync(string username)
        {
            var cookieData = new CookieData
            {
                Username = username,
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
            var appDataPath = await Electron.App.GetPathAsync(PathName.AppData);
            var filePath = Path.Combine(appDataPath, "Facilys", CookieFileName);

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
            var appDataPath = await Electron.App.GetPathAsync(PathName.AppData);
            var directoryPath = Path.Combine(appDataPath, "Facilys");
            var filePath = Path.Combine(directoryPath, CookieFileName);

            Directory.CreateDirectory(directoryPath);

            if (cookieData == null)
            {
                File.Delete(filePath);
                return;
            }

            var json = JsonSerializer.Serialize(cookieData);
            await File.WriteAllTextAsync(filePath, json);
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
    }

    public class CookieData
    {
        public string Username { get; set; }
        public bool IsConnected { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Key { get; set; }
    }
}