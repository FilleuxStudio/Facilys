using Microsoft.Data.Sqlite;
using System.ComponentModel.DataAnnotations;

namespace ClassLibraryFacilys.Models
{
    public class VersionDatabase
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string Version { get; set; } = "0";
        [Required]
        public DateTime DateVersion  { get; set; } = DateTime.Now;
        [Required]
        public string PathBackup { get; set; } = string.Empty;

        /// <summary>
        /// Crée une sauvegarde de la base de données SQLite.
        /// </summary>
        /// <param name="backupFolderPath">Le chemin du dossier où la sauvegarde sera stockée.</param>
        /// <param name="connectionString">La chaîne de connexion à la base de données source.</param>
        /// <returns>True si la sauvegarde a réussi, sinon False.</returns>
        public static bool BackupDatabase(string backupFolderPath, string connectionString)
        {
            string databasePath = new SqliteConnectionStringBuilder(connectionString).DataSource;
            string backupFileName = $"backup_{DateTime.Now:yyyyMMddHHmmss}.db";
            string backupFilePath = Path.Combine(backupFolderPath, backupFileName);

            try
            {
                using var sourceConnection = new SqliteConnection(connectionString);
                sourceConnection.Open();
                using var backupConnection = new SqliteConnection($"Data Source={backupFilePath}");
                backupConnection.Open();
                sourceConnection.BackupDatabase(backupConnection);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
