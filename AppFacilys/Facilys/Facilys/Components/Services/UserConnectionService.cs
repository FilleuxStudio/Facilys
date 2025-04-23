using ElectronNET.API;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Facilys.Components.Services
{
    public class UserConnectionService
    {
        private readonly ProtectedLocalStorage _localStorage;
        private const string StorageKey = "UserConnectionCredentials";
        /// <summary>
        /// Propriété retournant la chaîne de connexion à utiliser dans le DynamicDbContextFactory.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Adresse du serveur. Par défaut, nous fixons "localhost". 
        /// Vous pouvez aussi prévoir une option de configuration si nécessaire.
        /// </summary>
        public string Server { get; private set; } = "localhost";

        /// <summary>
        /// Nom de la base de données ciblée.
        /// </summary>
        public string Database { get; private set; }

        /// <summary>
        /// Identifiant de l'utilisateur.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Mot de passe de l'utilisateur.
        /// </summary>
        public string Password { get; private set; }

        public UserConnectionService(ProtectedLocalStorage localStorage)
        {
            _localStorage = localStorage;
            LoadCredentialsAsync().ConfigureAwait(false); // Charger au démarrage
        }


        /// <summary>
        /// Permet d’attribuer les informations de connexion récupérées via l’API et de construire la chaîne de connexion.
        /// Cette méthode peut être asynchrone si vous avez besoin d’effectuer des opérations avant de finaliser la configuration.
        /// </summary>
        /// <param name="database">Le nom de la base de données à utiliser (fourni par l’API).</param>
        /// <param name="userId">L’identifiant de connexion (fourni par l’API).</param>
        /// <param name="password">Le mot de passe de connexion (fourni par l’API).</param>
        public async Task<Task> SetCredentialsAsync(string database, string userId, string password)
        {
            Database = database;
            UserId = userId;
            Password = password;

            // Construire la chaîne de connexion avec les informations de connexion récupérées.
            // Vous pouvez adapter ce format selon votre fournisseur (MySQL/MariaDB ici).
            ConnectionString = $"Server={Server};Port=3306;Database={Database};Uid={UserId};Pwd={Password};";
            // Sauvegarder dans le stockage local
            await _localStorage.SetAsync(StorageKey, new { Database, UserId, Password });
            return Task.CompletedTask;
        }

        public async Task LoadCredentialsAsync()
        {
            try
            {
                if (HybridSupport.IsElectronActive == false)
                {
                    if (Database == null)
                    {
                        var result = await _localStorage.GetAsync<dynamic>(StorageKey);
                        if (result.Success)
                        {
                            var json = result.Value;

                            Database = json.GetProperty("database").GetString();
                            UserId = json.GetProperty("userId").GetString();
                            Password = json.GetProperty("password").GetString();

                            ConnectionString = $"Server={Server};Port=3306;Database={Database};Uid={UserId};Pwd={Password};";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement credentials: {ex.Message}");
            }
        }
    }
}
