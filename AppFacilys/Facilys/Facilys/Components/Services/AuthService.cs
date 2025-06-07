//using ElectronNET.API;
//using Facilys.Components.Constants;
//using Facilys.Components.Data;
//using Facilys.Components.Models;
//using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.Json;

//namespace Facilys.Components.Services
//{
//    public class AuthService
//    {
//        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
//        private const string CookieFileName = "cookieConnect.json";
//        private readonly APIWebSiteService _webSiteService;
//        private readonly ProtectedLocalStorage _localStorage;
//        private readonly UserConnectionService _userConnection;
//        public CookieData _cookieData { get; set; } = new();

//        public AuthService(IDbContextFactory<ApplicationDbContext> contextFactory, APIWebSiteService webSiteService, ProtectedLocalStorage localStorage, UserConnectionService userConnection)
//        {
//            _contextFactory = contextFactory;
//            _webSiteService = webSiteService;
//            _localStorage = localStorage;
//            _userConnection = userConnection;
//        }

//        public async Task<Users> AuthenticateAsync(string email, string password)
//        {
//            //await using var localContext = _factory.CreateDbContext();
//            //var user = await localContext.Users.FirstOrDefaultAsync(u => u.Email == email);


//            //await using var mariaContext = _factory.CreateDbContext();
//            //if (await mariaContext.Database.CanConnectAsync())
//            //{
//            //    return user;
//            //}


//            var result = await _webSiteService.PostConnectionUserAsync(email, password);
//            if (result.Success)
//            {
//                await SetMariaDBCredentials(result.UserData);

//                using var _context = await _contextFactory.CreateDbContextAsync();
//                Users userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
//                if (userDb == null)
//                {
//                    SetUserWeb(result.UserData);
//                    userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
//                }

//                await SetAuthenticatedAsync(userDb);
//                return userDb;
//            }
//            else
//            {
//                using var _context = await _contextFactory.CreateDbContextAsync();
//                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
//                if (user != null && Users.VerifyPassword(password, user.Password))
//                {
//                    await SetMariaDBCredentials(result.UserData);
//                    await SetAuthenticatedAsync(user);
//                    return user;
//                }
//            }
//            return null;
//        }

//        /// <summary>
//        /// Authentifie l'utilisateur en créant et sauvegardant un nouveau cookie.
//        /// </summary>
//        public async Task SetAuthenticatedAsync(Users user)
//        {
//            var cookieData = new CookieData
//            {
//                Id = user.Id,
//                Login = user.Login,
//                Email = user.Email,
//                IsConnected = true,
//                ExpirationDate = DateTime.UtcNow.AddDays(30),
//                Key = GenerateSha256(user.Email + DateTime.UtcNow.Ticks)
//            };
//            _cookieData = cookieData;
//            await SaveCookieDataAsync(cookieData);
//        }

//        /// <summary>
//        /// Sauvegarde les données du cookie.
//        /// Pour Electron, on sauvegarde dans un fichier JSON local, sinon on le place dans un cookie HTTP.
//        /// </summary>
//        private async Task SaveCookieDataAsync(CookieData cookieData)
//        {
//            if (HybridSupport.IsElectronActive)
//            {
//                var appDataPath = GetAppDataPath();
//                var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
//                var filePath = Path.Combine(directoryPath, CookieFileName);
//                try
//                {
//                    Directory.CreateDirectory(directoryPath);
//                    if (cookieData == null)
//                    {
//                        if (File.Exists(filePath))
//                            File.Delete(filePath);
//                        return;
//                    }
//                    string json = JsonSerializer.Serialize(cookieData, new JsonSerializerOptions { WriteIndented = true });
//                    await File.WriteAllTextAsync(filePath, json);
//                }
//                catch (Exception ex)
//                {
//                    Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
//                    throw;
//                }
//            }
//            await _localStorage.SetAsync("cookieData", _cookieData);
//        }

//        public static void EnsureApplicationFolderExists()
//        {
//            // Obtient le chemin des documents utilisateur via Electron
//            var documentsPath = GetAppDataPath();

//            // Combine le chemin pour inclure le dossier "Facilys"
//            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");

//            // Vérifie si le dossier existe, sinon le crée
//            if (!Directory.Exists(facilysFolderPath))
//            {
//                Directory.CreateDirectory(facilysFolderPath);
//            }
//        }


//        /// <summary>
//        /// Vérifie si l'utilisateur est authentifié en chargeant et validant les données du cookie.
//        /// </summary>
//        public async Task<bool> IsAuthenticatedAsync()
//        {
//            if (string.IsNullOrEmpty(EnvironmentApp.AccessToken))
//            {
//                var token = await _webSiteService.GetKeyAccessApp();
//                EnvironmentApp.AccessToken = token;
//            }

//            var cookieData = await GetAuthenticatedAsync();

//            return cookieData != null &&
//                   cookieData.IsConnected &&
//                   DateTime.UtcNow < cookieData.ExpirationDate;
//        }

//        /// <summary>
//        /// Déconnecte l'utilisateur en supprimant les données du cookie.
//        /// </summary>
//        public async Task LogoutAsync()
//        {
//            if (HybridSupport.IsElectronActive)
//            {
//                DeleteCookieData();
//            }

//            _cookieData = new CookieData();
//            await _localStorage.DeleteAsync("cookieData");
//        }

//        /// <summary>
//        /// Charge les données du cookie.
//        /// Pour Electron, on lit le fichier JSON local, sinon on récupère le cookie du navigateur.
//        /// </summary>
//        public async Task<CookieData> GetAuthenticatedAsync()
//        {
//            try
//            {
//                if (HybridSupport.IsElectronActive)
//                {
//                    var appDataPath = GetAppDataPath();
//                    var filePath = Path.Combine(appDataPath, EnvironmentApp.FolderData, CookieFileName);

//                    if (!File.Exists(filePath))
//                        return null;

//                    string json = await File.ReadAllTextAsync(filePath);
//                    if (string.IsNullOrWhiteSpace(json))
//                        return null;

//                    var CookieData = JsonSerializer.Deserialize<CookieData>(json);
//                    EnvironmentApp.EmailUserConnect = CookieData.Email;
//                    return CookieData;
//                }

//                var result = await _localStorage.GetAsync<CookieData>("cookieData");
//                if (result.Success)
//                {

//                    return _cookieData = result.Value;
//                }
//                return null;
//            }
//            catch (Exception ex)
//            {
//                Console.Error.WriteLine($"Erreur inattendue lors du chargement des cookies : {ex.Message}");
//                return null;
//            }
//        }


//        /// <summary>
//        /// Supprime les données du cookie.
//        /// </summary>
//        private static void DeleteCookieData()
//        {
//            if (HybridSupport.IsElectronActive)
//            {
//                var appDataPath = GetAppDataPath();
//                var directoryPath = Path.Combine(appDataPath, EnvironmentApp.FolderData);
//                var filePath = Path.Combine(directoryPath, CookieFileName);
//                try
//                {
//                    if (File.Exists(filePath))
//                        File.Delete(filePath);
//                }
//                catch (Exception ex)
//                {
//                    Console.Error.WriteLine($"Erreur lors de la suppression du cookie : {ex.Message}");
//                    throw;
//                }
//            }
//        }

//        public async void SetUserWeb(UserData user)
//        {
//            await using var _context = await _contextFactory.CreateDbContextAsync();
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

//        private async Task SetMariaDBCredentials(UserData user)
//        {
//            // Utilisez les informations de connexion fournies par l'API
//            //_userConnection.Server = "localhost";
//            //_userConnection.Database = user.MariadbDb;
//            //_userConnection.UserId = user.MariadbUser;
//            //_userConnection.Password = user.MariadbPassword;

//            await _userConnection.SetCredentialsAsync(
//            user.MariadbDb,
//            user.MariadbUser,
//            user.MariadbPassword
//            );
//        }

//        /// <summary>
//        /// Génère un hash SHA256 à partir d'une chaîne d'entrée.
//        /// </summary>
//        private static string GenerateSha256(string input)
//        {
//            using (SHA256 sha256 = SHA256.Create())
//            {
//                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
//                return Convert.ToBase64String(bytes);
//            }
//        }

//        /// <summary>
//        /// Vérifie et crée le dossier de l'application si nécessaire.
//        /// </summary>
//        public static async Task EnsureApplicationFolderExistsAsync()
//        {
//            var documentsPath = GetAppDataPath();
//            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");
//            if (!Directory.Exists(facilysFolderPath))
//            {
//                Directory.CreateDirectory(facilysFolderPath);
//            }
//        }

//        /// <summary>
//        /// Récupère le chemin AppData (ici, les documents) selon l'environnement (Electron ou non).
//        /// </summary>
//        private static string GetAppDataPath()
//        {
//            if (HybridSupport.IsElectronActive)
//            {
//                // Vous pouvez utiliser Electron.App.GetPathAsync si besoin
//                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
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
using Facilys.Components.Data;
using Facilys.Components.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Facilys.Components.Services
{
    public class AuthService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly ILogger<AuthService> _logger;
        private const string CookieFileName = "cookieConnect.json";
        private readonly APIWebSiteService _webSiteService;
        private readonly ProtectedLocalStorage _localStorage;
        private readonly UserConnectionService _userConnection;
        private readonly EnvironmentApp _envApp;
        public CookieData _cookieData { get; set; } = new();

        public AuthService(IDbContextFactory<ApplicationDbContext> contextFactory, APIWebSiteService webSiteService, ProtectedLocalStorage localStorage, UserConnectionService userConnection, EnvironmentApp envApp, ILogger<AuthService> logger)
        {
            _contextFactory = contextFactory;
            _webSiteService = webSiteService;
            _localStorage = localStorage;
            _userConnection = userConnection;
            _envApp = envApp;
            _logger = logger;
        }

        public async Task<Users> AuthenticateAsync(string email, string password)
        {
            var result = await _webSiteService.PostConnectionUserAsync(email, password);
            if (result.Success)
            {
                await SetMariaDBCredentials(result.UserData);
                using var _context = await _contextFactory.CreateDbContextAsync();
                Users userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (userDb == null)
                {
                    await SetUserWeb(result.UserData);
                    userDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                }

                await SetAuthenticatedAsync(userDb);
                return userDb;
            }
            else
            {
                using var _context = await _contextFactory.CreateDbContextAsync();
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null && Users.VerifyPassword(password, user.Password))
                {
                    await SetMariaDBCredentials(result.UserData);
                    await SetAuthenticatedAsync(user);
                    return user;
                }
            }
            return null;
        }

        /// <summary>
        /// Authentifie l'utilisateur en créant et sauvegardant un nouveau cookie avec expiration de 3 heures.
        /// </summary>
        public async Task SetAuthenticatedAsync(Users user)
        {
            var cookieData = new CookieData
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                IsConnected = true,
                ExpirationDate = DateTime.UtcNow.AddHours(3), // Changé de 30 jours à 3 heures
                Key = GenerateSha256(user.Email + DateTime.UtcNow.Ticks),
                LastActivity = DateTime.UtcNow // Nouveau champ pour suivre la dernière activité
            };
            _cookieData = cookieData;
            _envApp.EmailUserConnect = user.Email;
            await SaveCookieDataAsync(cookieData);
        }

        /// <summary>
        /// Met à jour la dernière activité de l'utilisateur pour maintenir la session active.
        /// </summary>
        public async Task UpdateLastActivityAsync()
        {
            if (_cookieData != null && _cookieData.IsConnected)
            {
                _cookieData.LastActivity = DateTime.UtcNow;
                // Étendre l'expiration de 3 heures à partir de maintenant
                _cookieData.ExpirationDate = DateTime.UtcNow.AddHours(3);
                await SaveCookieDataAsync(_cookieData);
            }
        }

        /// <summary>
        /// Sauvegarde les données du cookie.
        /// Pour Electron, on sauvegarde dans un fichier JSON local, sinon on le place dans le localStorage protégé.
        /// </summary>
        private async Task SaveCookieDataAsync(CookieData cookieData)
        {
            if (HybridSupport.IsElectronActive)
            {
                var appDataPath = GetAppDataPath();
                var directoryPath = Path.Combine(appDataPath, _envApp.FolderData);
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
                    string json = JsonSerializer.Serialize(cookieData, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(filePath, json);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Erreur lors de la sauvegarde du cookie : {ex.Message}");
                    throw;
                }
            }
            else
            {
                // Version web : utilisation du ProtectedLocalStorage
                try
                {
                    await _localStorage.SetAsync("cookieData", cookieData);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Erreur lors de la sauvegarde dans le localStorage : {ex.Message}");
                    // Fallback : essayer de sauvegarder sans protection
                    throw;
                }
            }
        }

        public static void EnsureApplicationFolderExists()
        {
            var documentsPath = GetAppDataPath();
            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");
            if (!Directory.Exists(facilysFolderPath))
            {
                Directory.CreateDirectory(facilysFolderPath);
            }
        }

        /// <summary>
        /// Vérifie si l'utilisateur est authentifié en chargeant et validant les données du cookie.
        /// Vérifie également si la session n'a pas expiré (3 heures).
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            if (string.IsNullOrEmpty(_envApp.AccessToken))
            {
                var token = await _webSiteService.GetKeyAccessApp();
                _envApp.AccessToken = token;
            }

            var cookieData = await GetAuthenticatedAsync();

            if (cookieData != null &&
                cookieData.IsConnected &&
                DateTime.UtcNow < cookieData.ExpirationDate)
            {
                // Session valide, mettre à jour la dernière activité
                await UpdateLastActivityAsync();
                return true;
            }

            // Session expirée ou invalide, nettoyer
            if (cookieData != null && DateTime.UtcNow >= cookieData.ExpirationDate)
            {
                await LogoutAsync();
            }

            return false;
        }

        /// <summary>
        /// Vérifie si l'utilisateur a une session valide sans mettre à jour l'activité.
        /// Utilisé pour les vérifications silencieuses.
        /// </summary>
        public async Task<bool> IsAuthenticatedSilentAsync()
        {
            var cookieData = await GetAuthenticatedAsync();

            return cookieData != null &&
                   cookieData.IsConnected &&
                   DateTime.UtcNow < cookieData.ExpirationDate;
        }

        /// <summary>
        /// Déconnecte l'utilisateur en supprimant toutes les données de session et cookies.
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                // 1. Supprimer les données locales
                if (HybridSupport.IsElectronActive)
                {
                    DeleteCookieData();
                }
                else
                {
                    try
                    {
                        await _localStorage.DeleteAsync("cookieData");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Erreur lors de la suppression du localStorage : {ex.Message}");
                    }
                }

                // 3. Nettoyer les données locales
                _cookieData = new CookieData();
                _envApp.EmailUserConnect = string.Empty;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur lors de la déconnexion complète : {ex.Message}");
            }
        }

        /// <summary>
        /// Charge les données du cookie.
        /// Pour Electron, on lit le fichier JSON local, sinon on récupère du localStorage protégé.
        /// </summary>
        public async Task<CookieData> GetAuthenticatedAsync()
        {
            try
            {
                if (HybridSupport.IsElectronActive)
                {
                    var appDataPath = GetAppDataPath();
                    var filePath = Path.Combine(appDataPath, _envApp.FolderData, CookieFileName);

                    if (!File.Exists(filePath))
                        return null;

                    string json = await File.ReadAllTextAsync(filePath);
                    if (string.IsNullOrWhiteSpace(json))
                        return null;

                    var cookieData = JsonSerializer.Deserialize<CookieData>(json);
                    if (cookieData != null)
                    {
                        _envApp.EmailUserConnect = cookieData.Email;
                        _cookieData = cookieData;
                    }
                    return cookieData;
                }
                else
                {
                    // Version web
                    var result = await _localStorage.GetAsync<CookieData>("cookieData");
                    if (result.Success && result.Value != null)
                    {
                        _envApp.EmailUserConnect = result.Value.Email;
                        _cookieData = result.Value;
                        return result.Value;
                    }
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
        /// Supprime les données du cookie pour Electron.
        /// </summary>
        private void DeleteCookieData()
        {
            if (HybridSupport.IsElectronActive)
            {
                var appDataPath = GetAppDataPath();
                var directoryPath = Path.Combine(appDataPath, _envApp.FolderData);
                var filePath = Path.Combine(directoryPath, CookieFileName);
                try
                {
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

        public async Task SetUserWeb(UserData user)
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
            await _context.Users.AddAsync(AddUserWeb);
            await _context.SaveChangesAsync();
        }

        private async Task SetMariaDBCredentials(UserData user)
        {
            await _userConnection.SetCredentialsAsync(
                user.MariadbDb,
                user.MariadbUser,
                user.MariadbPassword
            );
        }

        /// <summary>
        /// Génère un hash SHA256 à partir d'une chaîne d'entrée.
        /// </summary>
        private static string GenerateSha256(string input)
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
            var documentsPath = GetAppDataPath();
            var facilysFolderPath = Path.Combine(documentsPath, "Facilys");
            if (!Directory.Exists(facilysFolderPath))
            {
                Directory.CreateDirectory(facilysFolderPath);
            }
        }

        /// <summary>
        /// Récupère le chemin AppData selon l'environnement.
        /// </summary>
        private static string GetAppDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }

    public class CookieData
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsConnected { get; set; } = false;
        public DateTime ExpirationDate { get; set; } = DateTime.Now;
        public DateTime LastActivity { get; set; } = DateTime.Now; // Nouveau champ
        public string Key { get; set; } = string.Empty;
    }
}