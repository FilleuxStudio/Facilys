using ElectronNET.API;
using ElectronNET.API.Entities;
using Facilys.Components.Constants;
using Facilys.Components.Data;
using Facilys.Components.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Facilys.Components.Services
{
    public class AuthService
    {
        private const string CookieName = "FacilysAuthCookie";
        private const string CookieFileName = "cookieConnect.json";
        private const int CookieValidityDays = 30;
        private CookieData cookieUserConnect;
        private readonly ApplicationDbContext _context;
        private readonly APIWebSiteService _webSiteService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthService(ApplicationDbContext context, APIWebSiteService webSiteService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _webSiteService = webSiteService;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<Users> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null && Users.VerifyPassword(password, user.Password))
            {
                await SetAuthenticatedAsync(user);
                return user;
            }
            else
            {
                var result = await _webSiteService.PostConnectionUserAsync(email, password);
                if (result.Success)
                {
                    Users userDb = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();

                    if (userDb == null)
                    {
                        SetUserWeb(result.UserData);
                        userDb = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
                    }

                    await SetAuthenticatedAsync(userDb);
                    return userDb;
                }
            }
            return null;
        }

        /// <summary>
        /// Vérifie si l'utilisateur est authentifié en chargeant et validant les données du cookie.
        /// Utilise : System.Text.Json pour la désérialisation.
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            if (EnvironmentApp.AccessToken == "")
            {
                var token = await _webSiteService.GetKeyAccessApp();
                EnvironmentApp.AccessToken = token;
            }

            var cookieData = await LoadCookieDataAsync();
            if (cookieData == null) return false;

            return cookieData.IsConnected &&
                   DateTime.UtcNow < cookieData.ExpirationDate;
        }

        /// <summary>
        /// Authentifie l'utilisateur en créant et sauvegardant un nouveau cookie.
        /// Utilise : System.Security.Cryptography pour générer la clé SHA256.
        /// </summary>
        public async Task SetAuthenticatedAsync(Users user)
        {
            // bool flag = await _webSiteService.PostConnectionUserAsync(user.Email, user.Password);

            var cookieData = new CookieData
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                IsConnected = true,
                ExpirationDate = DateTime.UtcNow.AddDays(CookieValidityDays),
                Key = GenerateSha256(user.Email + DateTime.UtcNow.Ticks)
            };
            cookieUserConnect = cookieData;
            await SaveCookieDataAsync(cookieData);
        }

        public async Task<CookieData> GetAuthenticatedAsync()
        {
            cookieUserConnect = await LoadCookieDataAsync();
            return cookieUserConnect;
        }

        /// <summary>
        /// Déconnecte l'utilisateur en supprimant les données du cookie.
        /// </summary>
        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName);
            DeleteCookieDataAsync();
        }

        /// <summary>
        /// Charge les données du cookie depuis le fichier JSON.
        /// Utilise : ElectronNET.API pour accéder au chemin AppData,
        /// System.Text.Json pour la désérialisation.
        /// </summary>
        private async Task<CookieData> LoadCookieDataAsync()
        {
            var appDataPath = await GetAppDataPathAsync();

            var filePath = Path.Combine(appDataPath, EnvironmentApp.FolderData, CookieFileName);

            if (!File.Exists(filePath)) return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<CookieData>(json);

            //if (HybridSupport.IsElectronActive)
            //{
            //    var appDataPath = await GetAppDataPathAsync();

            //    var filePath = Path.Combine(appDataPath, EnvironmentApp.FolderData, CookieFileName);

            //    if (!File.Exists(filePath)) return null;

            //    var json = await File.ReadAllTextAsync(filePath);
            //    return JsonSerializer.Deserialize<CookieData>(json);
            //}
            //else
            //{
            //    var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
            //    if (string.IsNullOrEmpty(cookie)) return null;

            //    try
            //    {
            //        return JsonSerializer.Deserialize<CookieData>(cookie);
            //    }
            //    catch
            //    {
            //        return null; // En cas d'erreur de désérialisation, retourne null
            //    }
            //}
        }

        /// <summary>
        /// Sauvegarde les données du cookie dans un fichier JSON.
        /// Utilise : ElectronNET.API pour accéder au chemin AppData,
        /// System.Text.Json pour la sérialisation.
        /// </summary>
        private async Task SaveCookieDataAsync(CookieData cookieData)
        {
            // Récupère le chemin "Documents"
            var appDataPath = await GetAppDataPathAsync();

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

                //var cookieOptions = new CookieOptions
                //{
                //    Expires = cookieData.ExpirationDate,
                //    HttpOnly = true,
                //    Secure = true, // Utilise HTTPS pour sécuriser le cookie
                //    SameSite = SameSiteMode.Strict
                //};
                //_httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieName, json, cookieOptions);
            }
            catch (Exception ex)
            {
                // Gère les exceptions éventuelles (par exemple, accès refusé)
                Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
                throw;
            }
        }

        private async void DeleteCookieDataAsync()
        {
            // Récupère le chemin "Documents"
            var appDataPath = await GetAppDataPathAsync();

            // Combine le chemin pour le dossier de l'application et le fichier cookie
            var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
            var filePath = Path.Combine(directoryPath, CookieFileName);

            try
            {
                // Assure que le répertoire existe
                Directory.CreateDirectory(directoryPath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // Gère les exceptions éventuelles (par exemple, accès refusé)
                Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
                throw;
            }
        }

        public void SetUserWeb(UserData user)
        {
            var AddUserWeb = new Users
            {
                Lname = user.Lname,
                Fname = user.Fname,
                Email = user.Email,
                Login = user.Email,
                Password = user.Password,
                Role = user.Role,
                Team = user.Company,
                DateAdded = DateTime.Now
            };

            _context.Users.Add(AddUserWeb);
            _context.SaveChanges();
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

        public static async Task EnsureApplicationFolderExists()
        {
            // Obtient le chemin des documents utilisateur via Electron
            var documentsPath = await GetAppDataPathAsync();

            // Combine le chemin pour inclure le dossier "Facilys"
            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");

            // Vérifie si le dossier existe, sinon le crée
            if (!Directory.Exists(facilysFolderPath))
            {
                Directory.CreateDirectory(facilysFolderPath);
            }
        }

        // Utiliser Electron en priorité, avec fallback .NET
        private static async Task<string> GetAppDataPathAsync()
        {
            if (HybridSupport.IsElectronActive)
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //return await Electron.App.GetPathAsync(PathName.Documents); 
            }
            else
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
        }
    }

    public class CookieData
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsConnected { get; set; } = false;
        public DateTime ExpirationDate { get; set; } = DateTime.Now;
        public string Key { get; set; } = string.Empty;
    }
}