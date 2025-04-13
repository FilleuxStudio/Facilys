namespace Facilys.Components.Services
{
    public class UserConnectionService
    {
        /// <summary>
        /// Propriété retournant la chaîne de connexion à utiliser dans le DynamicDbContextFactory.
        /// </summary>
        public string ConnectionString { get; private set; } = string.Empty;

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

        /// <summary>
        /// Permet d’attribuer les informations de connexion récupérées via l’API et de construire la chaîne de connexion.
        /// Cette méthode peut être asynchrone si vous avez besoin d’effectuer des opérations avant de finaliser la configuration.
        /// </summary>
        /// <param name="database">Le nom de la base de données à utiliser (fourni par l’API).</param>
        /// <param name="userId">L’identifiant de connexion (fourni par l’API).</param>
        /// <param name="password">Le mot de passe de connexion (fourni par l’API).</param>
        public Task SetCredentialsAsync(string database, string userId, string password)
        {
            Database = database;
            UserId = userId;
            Password = password;

            // Construire la chaîne de connexion avec les informations de connexion récupérées.
            // Vous pouvez adapter ce format selon votre fournisseur (MySQL/MariaDB ici).
            ConnectionString = $"Server={Server};Port=3306;Database={Database};Uid={UserId};Pwd={Password};";

            return Task.CompletedTask;
        }
    }
}
