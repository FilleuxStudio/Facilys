using ElectronNET.API;
using Facilys.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using Renci.SshNet;

namespace Facilys.Components.Data
{
    public class DynamicDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        UserConnectionService _userConnectionService;

        public DynamicDbContextFactory(IServiceProvider serviceProvider, IConfiguration configuration, UserConnectionService userConnectionService)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _userConnectionService = userConnectionService;
        }

        public ApplicationDbContext CreateDbContext()
        {
            // Création d'un scope pour les services scoped
            using var scope = _serviceProvider.CreateScope();
            //var userConnectionService = scope.ServiceProvider.GetRequiredService<UserConnectionService>();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            try
            {
                if (!string.IsNullOrEmpty(_userConnectionService.ConnectionString))
                {
                    // Configuration du tunnel SSH
                    var sshTunnel = new SshTunnelService(_configuration);
                    sshTunnel.Start();


                    var forwardedConnectionString = _userConnectionService.ConnectionString.Replace("Server=localhost", "Server=127.0.0.1").Replace("Port=3306", $"Port={sshTunnel.LocalPort}");

                    var serverVersion = new MariaDbServerVersion(new Version(10, 6, 21));

                    optionsBuilder.UseMySql(
                        forwardedConnectionString,
                        serverVersion,
                        options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null));
                }
                else if (HybridSupport.IsElectronActive)
                {
                    // Mode déconnecté - SQLite
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
                // Log et fallback
                Console.WriteLine($"Erreur création DbContext: {ex.Message}");

                if (!HybridSupport.IsElectronActive) throw;

                // Fallback SQLite
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
                Directory.CreateDirectory(facilysPath);
                optionsBuilder.UseSqlite($"Data Source={Path.Combine(facilysPath, "data.db")}");
                return new ApplicationDbContext(optionsBuilder.Options);
            }
        }
    }

    // Service dédié pour la gestion SSH
    public class SshTunnelService : IDisposable
    {
        private readonly IConfiguration _configuration;
        private SshClient _sshClient;
        private ForwardedPortLocal _forwardedPort;
        public int LocalPort { get; private set; }

        public SshTunnelService(IConfiguration configuration)
        {
            _configuration = configuration;
            LocalPort = new Random().Next(5000, 7000);
        }

        public void Start()
        {
            var sshHost = _configuration["Ssh:Host"];
            var sshPort = int.Parse(_configuration["Ssh:Port"] ?? "22");
            var sshUser = _configuration["Ssh:Username"];
            var sshPassword = _configuration["Ssh:Password"];

            _sshClient = new SshClient(sshHost, sshPort, sshUser, sshPassword);
            _sshClient.Connect();

            _forwardedPort = new ForwardedPortLocal("127.0.0.1", (uint)LocalPort, "127.0.0.1", 3306);
            _sshClient.AddForwardedPort(_forwardedPort);
            _forwardedPort.Start();

            if (!_sshClient.IsConnected)
            {
                throw new Exception("Échec de la connexion SSH");
            }

        }

        public void Dispose()
        {
            _forwardedPort?.Stop();
            _forwardedPort?.Dispose();
            _sshClient?.Disconnect();
            _sshClient?.Dispose();
        }
    }
}