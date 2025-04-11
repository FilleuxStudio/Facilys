using ElectronNET.API;
using Facilys.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Renci.SshNet;

namespace Facilys.Components.Data
{
    public class DynamicDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly IServiceProvider _provider;
        private SshClient _sshClient;
        private ForwardedPortLocal _forwardedPort;
        private readonly UserConnectionService _userConnection;

        public DynamicDbContextFactory(IServiceProvider provider, UserConnectionService userConnection)
        {
            _userConnection = userConnection;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var scope = _provider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            try
            {
                if (!string.IsNullOrEmpty(_userConnection.ConnectionString))
                {
                    // Mode connecté - MariaDB via SSH
                    SetupSshTunnel(configuration);

                    // Utiliser le port local forwardé (5022 dans votre exemple)
                    var forwardedConnectionString = _userConnection.ConnectionString
                        .Replace("Server=localhost", "Server=127.0.0.1")
                        .Replace("Port=3306", "Port=5022");

                    optionsBuilder.UseMySql(
                        forwardedConnectionString,
                        ServerVersion.AutoDetect(forwardedConnectionString),
                        options => options.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null));
                }
                else if (HybridSupport.IsElectronActive)
                {
                    // Mode déconnecté - SQLite local
                    var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
                    Directory.CreateDirectory(facilysPath);
                    optionsBuilder.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}");
                }
                else
                {
                    throw new InvalidOperationException("Aucune configuration de base de données valide");
                }

                return new ApplicationDbContext(optionsBuilder.Options);
            }
            catch (Exception ex)
            {
                // Nettoyer le tunnel SSH en cas d'erreur
                CleanupSshTunnel();

                // Log l'erreur et fallback sur SQLite
                Console.WriteLine($"Erreur création DbContext: {ex.Message}");

                if (!HybridSupport.IsElectronActive) throw;

                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
                Directory.CreateDirectory(facilysPath);
                optionsBuilder.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}");

                return new ApplicationDbContext(optionsBuilder.Options);
            }
            finally
            {
                scope.Dispose();
            }
        }

        private void SetupSshTunnel(IConfiguration configuration)
        {
            // Création du tunnel SSH
            var sshHost = configuration["Ssh:Host"];
            var sshPort = int.Parse(configuration["Ssh:Port"] ?? "5022");
            var sshUser = configuration["Ssh:Username"];
            var sshPassword = configuration["Ssh:Password"];

            _sshClient = new SshClient(sshHost, sshPort, sshUser, sshPassword);
            _sshClient.Connect();

            _forwardedPort = new ForwardedPortLocal("127.0.0.1", 5022, "127.0.0.1", 3306);
            _sshClient.AddForwardedPort(_forwardedPort);
            _forwardedPort.Start();
        }

        private void CleanupSshTunnel()
        {
            try
            {
                _forwardedPort?.Stop();
                _forwardedPort?.Dispose();
                _sshClient?.Disconnect();
                _sshClient?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du nettoyage du tunnel SSH: {ex.Message}");
            }
        }
    }
}