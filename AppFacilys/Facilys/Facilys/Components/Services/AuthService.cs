//using ElectronNET.API;
//using ElectronNET.API.Entities;
//using Facilys.Components.Constants;
//using Facilys.Components.Data;
//using Facilys.Components.Models;
//using Microsoft.AspNetCore.Components;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using static System.Runtime.InteropServices.JavaScript.JSType;

//namespace Facilys.Components.Services
//{
//    public class AuthService
//    {
//        private const string CookieName = "FacilysAuthCookie";
//        private const string CookieFileName = "cookieConnect.json";
//        private const int CookieValidityDays = 30;
//        private CookieData cookieUserConnect;
//        private readonly ApplicationDbContext _context;
//        private readonly APIWebSiteService _webSiteService;
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        public AuthService(ApplicationDbContext context, APIWebSiteService webSiteService, IHttpContextAccessor httpContextAccessor)
//        {
//            _context = context;
//            _webSiteService = webSiteService;
//            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
//        }

//        public async Task<Users> AuthenticateAsync(string email, string password)
//        {
//            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

//            if (user != null && Users.VerifyPassword(password, user.Password))
//            {
//                await SetAuthenticatedAsync(user);
//                return user;
//            }
//            else
//            {
//                var result = await _webSiteService.PostConnectionUserAsync(email, password);
//                if (result.Success)
//                {
//                    Users userDb = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();

//                    if (userDb == null)
//                    {
//                        SetUserWeb(result.UserData);
//                        userDb = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
//                    }

//                    await SetAuthenticatedAsync(userDb);
//                    return userDb;
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// Vérifie si l'utilisateur est authentifié en chargeant et validant les données du cookie.
//        /// Utilise : System.Text.Json pour la désérialisation.
//        /// </summary>
//        public async Task<bool> IsAuthenticatedAsync()
//        {
//            if (EnvironmentApp.AccessToken == "")
//            {
//                var token = await _webSiteService.GetKeyAccessApp();
//                EnvironmentApp.AccessToken = token;
//            }

//            var cookieData = await LoadCookieDataAsync();
//            if (cookieData == null) return false;

//            return cookieData.IsConnected &&
//                   DateTime.UtcNow < cookieData.ExpirationDate;
//        }

//        /// <summary>
//        /// Authentifie l'utilisateur en créant et sauvegardant un nouveau cookie.
//        /// Utilise : System.Security.Cryptography pour générer la clé SHA256.
//        /// </summary>
//        public async Task SetAuthenticatedAsync(Users user)
//        {
//            // bool flag = await _webSiteService.PostConnectionUserAsync(user.Email, user.Password);

//            var cookieData = new CookieData
//            {
//                Id = user.Id,
//                Login = user.Login,
//                Email = user.Email,
//                IsConnected = true,
//                ExpirationDate = DateTime.UtcNow.AddDays(CookieValidityDays),
//                Key = GenerateSha256(user.Email + DateTime.UtcNow.Ticks)
//            };
//            cookieUserConnect = cookieData;
//            await SaveCookieDataAsync(cookieData);
//        }

//        public async Task<CookieData> GetAuthenticatedAsync()
//        {
//            cookieUserConnect = await LoadCookieDataAsync();
//            return cookieUserConnect;
//        }

//        /// <summary>
//        /// Déconnecte l'utilisateur en supprimant les données du cookie.
//        /// </summary>
//        public void Logout()
//        {
//            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(CookieName);
//            DeleteCookieDataAsync();
//        }

//        /// <summary>
//        /// Charge les données du cookie depuis le fichier JSON.
//        /// Utilise : ElectronNET.API pour accéder au chemin AppData,
//        /// System.Text.Json pour la désérialisation.
//        /// </summary>
//        private async Task<CookieData> LoadCookieDataAsync()
//        {
//            var appDataPath = await GetAppDataPathAsync();

//            var filePath = Path.Combine(appDataPath, EnvironmentApp.FolderData, CookieFileName);

//            if (!File.Exists(filePath)) return null;

//            var json = await File.ReadAllTextAsync(filePath);
//            return JsonSerializer.Deserialize<CookieData>(json);

//            //if (HybridSupport.IsElectronActive)
//            //{
//            //    var appDataPath = await GetAppDataPathAsync();

//            //    var filePath = Path.Combine(appDataPath, EnvironmentApp.FolderData, CookieFileName);

//            //    if (!File.Exists(filePath)) return null;

//            //    var json = await File.ReadAllTextAsync(filePath);
//            //    return JsonSerializer.Deserialize<CookieData>(json);
//            //}
//            //else
//            //{
//            //    var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[CookieName];
//            //    if (string.IsNullOrEmpty(cookie)) return null;

//            //    try
//            //    {
//            //        return JsonSerializer.Deserialize<CookieData>(cookie);
//            //    }
//            //    catch
//            //    {
//            //        return null; // En cas d'erreur de désérialisation, retourne null
//            //    }
//            //}
//        }

//        /// <summary>
//        /// Sauvegarde les données du cookie dans un fichier JSON.
//        /// Utilise : ElectronNET.API pour accéder au chemin AppData,
//        /// System.Text.Json pour la sérialisation.
//        /// </summary>
//        private async Task SaveCookieDataAsync(CookieData cookieData)
//        {
//            // Récupère le chemin "Documents"
//            var appDataPath = await GetAppDataPathAsync();

//            // Combine le chemin pour le dossier de l'application et le fichier cookie
//            var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
//            var filePath = Path.Combine(directoryPath, CookieFileName);

//            try
//            {
//                // Assure que le répertoire existe
//                Directory.CreateDirectory(directoryPath);

//                if (cookieData == null)
//                {
//                    // Supprime le fichier s'il existe
//                    if (File.Exists(filePath))
//                    {
//                        File.Delete(filePath);
//                    }
//                    return;
//                }

//                // Sérialise les données du cookie en JSON
//                var json = JsonSerializer.Serialize(cookieData, new JsonSerializerOptions
//                {
//                    WriteIndented = true // Facilite la lecture du fichier
//                });

//                // Écrit le JSON dans le fichier
//                await File.WriteAllTextAsync(filePath, json);

//                //var cookieOptions = new CookieOptions
//                //{
//                //    Expires = cookieData.ExpirationDate,
//                //    HttpOnly = true,
//                //    Secure = true, // Utilise HTTPS pour sécuriser le cookie
//                //    SameSite = SameSiteMode.Strict
//                //};
//                //_httpContextAccessor.HttpContext?.Response.Cookies.Append(CookieName, json, cookieOptions);
//            }
//            catch (Exception ex)
//            {
//                // Gère les exceptions éventuelles (par exemple, accès refusé)
//                Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
//                throw;
//            }
//        }

//        private async void DeleteCookieDataAsync()
//        {
//            // Récupère le chemin "Documents"
//            var appDataPath = await GetAppDataPathAsync();

//            // Combine le chemin pour le dossier de l'application et le fichier cookie
//            var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
//            var filePath = Path.Combine(directoryPath, CookieFileName);

//            try
//            {
//                // Assure que le répertoire existe
//                Directory.CreateDirectory(directoryPath);

//                if (File.Exists(filePath))
//                {
//                    File.Delete(filePath);
//                }
//            }
//            catch (Exception ex)
//            {
//                // Gère les exceptions éventuelles (par exemple, accès refusé)
//                Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
//                throw;
//            }
//        }

//        public void SetUserWeb(UserData user)
//        {
//            var AddUserWeb = new Users
//            {
//                Lname = user.Lname,
//                Fname = user.Fname,
//                Email = user.Email,
//                Login = user.Email,
//                Password = user.Password,
//                Role = user.Role,
//                Team = user.Company,
//                DateAdded = DateTime.Now
//            };

//            _context.Users.Add(AddUserWeb);
//            _context.SaveChanges();
//        }

//        /// <summary>
//        /// Génère un hash SHA256 à partir d'une chaîne d'entrée.
//        /// Utilise : System.Security.Cryptography pour le hachage SHA256,
//        /// System.Text.Encoding pour la conversion de chaîne en bytes.
//        /// </summary>
//        private string GenerateSha256(string input)
//        {
//            using (SHA256 sha256 = SHA256.Create())
//            {
//                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
//                return Convert.ToBase64String(bytes);
//            }
//        }

//        public static async Task EnsureApplicationFolderExists()
//        {
//            // Obtient le chemin des documents utilisateur via Electron
//            var documentsPath = await GetAppDataPathAsync();

//            // Combine le chemin pour inclure le dossier "Facilys"
//            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");

//            // Vérifie si le dossier existe, sinon le crée
//            if (!Directory.Exists(facilysFolderPath))
//            {
//                Directory.CreateDirectory(facilysFolderPath);
//            }
//        }

//        // Utiliser Electron en priorité, avec fallback .NET
//        private static async Task<string> GetAppDataPathAsync()
//        {
//            if (HybridSupport.IsElectronActive)
//            {
//                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//                //return await Electron.App.GetPathAsync(PathName.Documents); 
//            }
//            else
//            {
//                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//            }
//        }
//    }

//    public class CookieData
//    {
//        public Guid Id { get; set; } = Guid.Empty;
//        public string Login { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public bool IsConnected { get; set; } = false;
//        public DateTime ExpirationDate { get; set; } = DateTime.Now;
//        public string Key { get; set; } = string.Empty;
//    }
//}

using ElectronNET.API;
using ElectronNET.API.Entities;
using Facilys.Components.Constants;
using Facilys.Components.Data;
using Facilys.Components.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OpenCvSharp;
using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Facilys.Components.Services
{
    public class AuthService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private const string CookieFileName = "cookieConnect.json";
        private readonly APIWebSiteService _webSiteService;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly UserConnectionService _userConnection;
        public CookieData _cookieData { get; set; } = new();

        public AuthService(IDbContextFactory<ApplicationDbContext> contextFactory, APIWebSiteService webSiteService, ProtectedLocalStorage localStorage, UserConnectionService userConnection)
        {
            _contextFactory = contextFactory;
            _webSiteService = webSiteService;
            _localStorage = localStorage;
            _userConnection = userConnection;
        }

        public async Task<Users> AuthenticateAsync(string email, string password)
        {
            //await using var localContext = _factory.CreateDbContext();
            //var user = await localContext.Users.FirstOrDefaultAsync(u => u.Email == email);


            //await using var mariaContext = _factory.CreateDbContext();
            //if (await mariaContext.Database.CanConnectAsync())
            //{
            //    return user;
            //}
            await using var _context = await _contextFactory.CreateDbContextAsync();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && Users.VerifyPassword(password, user.Password))
            {
               await SetMariaDBCredentials(user);
               await SetAuthenticatedAsync(user);
                return user;
            }
            else
            {
                var result = await _webSiteService.PostConnectionUserAsync(email, password);
                if (result.Success)
                {
                    Users userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (userDb == null)
                    {
                       SetUserWeb(result.UserData);
                        userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    }

                   await SetMariaDBCredentials(user);
                   await SetAuthenticatedAsync(userDb);
                    return userDb;
                }
            }
            return null;
        }

        /// <summary>
        /// Authentifie l'utilisateur en créant et sauvegardant un nouveau cookie.
        /// </summary>
        public async Task SetAuthenticatedAsync(Users user)
        {
            var cookieData = new CookieData
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                IsConnected = true,
                ExpirationDate = DateTime.UtcNow.AddDays(30),
                Key = GenerateSha256(user.Email + DateTime.UtcNow.Ticks)
            };
            _cookieData = cookieData;
            await SaveCookieDataAsync(cookieData);
        }

        /// <summary>
        /// Sauvegarde les données du cookie.
        /// Pour Electron, on sauvegarde dans un fichier JSON local, sinon on le place dans un cookie HTTP.
        /// </summary>
        private async Task SaveCookieDataAsync(CookieData cookieData)
        {
            if (HybridSupport.IsElectronActive)
            {
                var appDataPath = await GetAppDataPathAsync();
                var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
                var filePath = Path.Combine(directoryPath, CookieFileName);
                try
                {
                    Directory.CreateDirectory(directoryPath);
                    if (cookieData == null)
                    {
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                        return;
                    }
                    var json = JsonSerializer.Serialize(cookieData, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
                    throw;
                }
            }
            await _localStorage.SetAsync("cookieData", _cookieData);
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


        /// <summary>
        /// Vérifie si l'utilisateur est authentifié en chargeant et validant les données du cookie.
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            if (string.IsNullOrEmpty(EnvironmentApp.AccessToken))
            {
                var token = await _webSiteService.GetKeyAccessApp();
                EnvironmentApp.AccessToken = token;
            }

            var cookieData = await GetAuthenticatedAsync();
            return cookieData != null &&
                   cookieData.IsConnected &&
                   DateTime.UtcNow < cookieData.ExpirationDate;
        }

        /// <summary>
        /// Déconnecte l'utilisateur en supprimant les données du cookie.
        /// </summary>
        public async Task LogoutAsync()
        {
            if (HybridSupport.IsElectronActive)
            {
                await DeleteCookieDataAsync();
            }

            _cookieData = new CookieData();
            await _localStorage.DeleteAsync("cookieData");
        }

        /// <summary>
        /// Charge les données du cookie.
        /// Pour Electron, on lit le fichier JSON local, sinon on récupère le cookie du navigateur.
        /// </summary>
        public async Task<CookieData> GetAuthenticatedAsync()
        {
            try
            {
                if (HybridSupport.IsElectronActive)
                {
                    var appDataPath = await GetAppDataPathAsync();
                    var filePath = Path.Combine(appDataPath, EnvironmentApp.FolderData, CookieFileName);

                    if (!File.Exists(filePath))
                        return null;

                    string json = await File.ReadAllTextAsync(filePath);
                    if (string.IsNullOrWhiteSpace(json))
                        return null;

                    return JsonSerializer.Deserialize<CookieData>(json);
                }

                var result = await _localStorage.GetAsync<CookieData>("cookieData");
                if (result.Success)
                {
                    return _cookieData = result.Value;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur inattendue lors du chargement des cookies : {ex.Message}");
                return null;
            }
        }


        /// <summary>
        /// Supprime les données du cookie.
        /// </summary>
        private async Task DeleteCookieDataAsync()
        {
            if (HybridSupport.IsElectronActive)
            {
                var appDataPath = await GetAppDataPathAsync();
                var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
                var filePath = Path.Combine(directoryPath, CookieFileName);
                try
                {
                    Directory.CreateDirectory(directoryPath);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Erreur lors de la suppression du cookie : {ex.Message}");
                    throw;
                }
            }
        }

        public async void SetUserWeb(UserData user)
        {
            await using var _context = await _contextFactory.CreateDbContextAsync();
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

        private async Task SetMariaDBCredentials(Users user)
        {
            // Exemple - adapter selon votre modèle de données
            _userConnection.Server = "user.DbServer";
            _userConnection.Database = "user.DbName";
            _userConnection.UserId = "user.DbUser";
            _userConnection.Password = "user.DbPassword";
        }

        /// <summary>
        /// Génère un hash SHA256 à partir d'une chaîne d'entrée.
        /// </summary>
        private string GenerateSha256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }

        /// <summary>
        /// Vérifie et crée le dossier de l'application si nécessaire.
        /// </summary>
        public static async Task EnsureApplicationFolderExistsAsync()
        {
            var documentsPath = await GetAppDataPathAsync();
            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");
            if (!Directory.Exists(facilysFolderPath))
            {
                Directory.CreateDirectory(facilysFolderPath);
            }
        }

        /// <summary>
        /// Récupère le chemin AppData (ici, les documents) selon l'environnement (Electron ou non).
        /// </summary>
        private static async Task<string> GetAppDataPathAsync()
        {
            if (HybridSupport.IsElectronActive)
            {
                // Vous pouvez utiliser Electron.App.GetPathAsync si besoin
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
