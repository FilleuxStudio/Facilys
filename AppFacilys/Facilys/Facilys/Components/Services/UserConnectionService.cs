using System;
using System.Text.Json;
using System.Threading.Tasks;
using ElectronNET.API;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Facilys.Components.Services
{
    public class UserConnectionService
    {
        private readonly ProtectedLocalStorage _localStorage;
        private const string StorageKey = "UserConnectionCredentials";

        /// <summary>
        /// Chaîne de connexion construite.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Adresse du serveur (par défaut "localhost").
        /// </summary>
        public string Server { get; private set; } = "localhost";

        /// <summary>
        /// Nom de la base de données ciblée.
        /// </summary>
        public string Database { get; private set; }

        /// <summary>
        /// Identifiant de l’utilisateur.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Mot de passe de l’utilisateur.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Injection de ProtectedLocalStorage.
        /// </summary>
        public UserConnectionService(ProtectedLocalStorage localStorage)
        {
            _localStorage = localStorage;
            // Ne pas appeler LoadCredentialsAsync() ici (pas d'async dans un constructeur) :contentReference[oaicite:4]{index=4}
        }

        /// <summary>
        /// Définit et persiste les credentials.
        /// </summary>
        public async Task SetCredentialsAsync(string database, string userId, string password)
        {
            Database = database;
            UserId = userId;
            Password = password;
            ConnectionString = $"Server={Server};Port=3306;Database={Database};Uid={UserId};Pwd={Password};";

            // Stockage chiffré dans le navigateur
            var dto = new { Database, UserId, Password };
            await _localStorage.SetAsync(StorageKey, dto);
        }

        /// <summary>
        /// Charge les credentials depuis le stockage local chiffré.
        /// Appeler explicitement depuis un composant (OnInitializedAsync).
        /// </summary>
        public async Task LoadCredentialsAsync()
        {
            try
            {
                if (HybridSupport.IsElectronActive == false)
                {
                    var result = await _localStorage.GetAsync<JsonElement>(StorageKey);
                    if (result.Success && result.Value.ValueKind == JsonValueKind.Object)
                    {
                        var json = result.Value;
                        Database = json.GetProperty("database").GetString();
                        UserId = json.GetProperty("userId").GetString();
                        Password = json.GetProperty("password").GetString();

                        ConnectionString = $"Server={Server};Port=3306;Database={Database};Uid={UserId};Pwd={Password};";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur chargement credentials: {ex.Message}");
            }
        }
    }
}
