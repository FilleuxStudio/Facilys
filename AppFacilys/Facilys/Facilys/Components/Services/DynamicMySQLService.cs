using MySql.Data.MySqlClient;
using System.Data;

namespace Facilys.Components.Services
{
    public class DynamicMySQLService
    {
        private string _connectionString;

        /// <summary>
        /// Initialise la connexion avec des paramètres utilisateur.
        /// </summary>
        /// <param name="server">Adresse du serveur MySQL</param>
        /// <param name="database">Nom de la base de données</param>
        /// <param name="userId">Identifiant utilisateur MySQL</param>
        /// <param name="password">Mot de passe utilisateur MySQL</param>
        /// <param name="port">Port du serveur MySQL (par défaut : 3306)</param>
        public void InitializeConnection(string server, string database, string userId, string password, int port = 3306)
        {
            _connectionString = $"Server={server};Port={port};Database={database};Uid={userId};Pwd={password};";
        }

        /// <summary>
        /// Teste la connexion à la base de données.
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection.State == ConnectionState.Open;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la connexion : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Exécute une requête SQL et retourne les résultats sous forme de DataTable.
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string query)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(query, connection);
                using var adapter = new MySqlDataAdapter(command);

                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                return dataTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution de la requête : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Exécute une commande SQL (INSERT, UPDATE, DELETE).
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string commandText)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new MySqlCommand(commandText, connection);
                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'exécution de la commande : {ex.Message}");
                throw;
            }
        }
    }
}
