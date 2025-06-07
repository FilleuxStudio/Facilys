using ElectronNET.API;
using Facilys.Components.Services;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Data
{
    public class OptimizedDynamicDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly UserConnectionService _userConnectionService;
        private readonly OptimizedSshTunnelService _sshTunnelService;
        private readonly ILogger<OptimizedDynamicDbContextFactory> _logger;

        // Cache des options pour éviter de les reconstruire
        private static DbContextOptions<ApplicationDbContext> _cachedMariaDbOptions;
        private static DbContextOptions<ApplicationDbContext> _cachedSqliteOptions;
        private static readonly object _lockObject = new object();

        public OptimizedDynamicDbContextFactory(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            UserConnectionService userConnectionService,
            OptimizedSshTunnelService sshTunnelService,
            ILogger<OptimizedDynamicDbContextFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _userConnectionService = userConnectionService;
            _sshTunnelService = sshTunnelService;
            _logger = logger;
        }

        public ApplicationDbContext CreateDbContext()
        {
            try
            {
                if (HybridSupport.IsElectronActive)
                {
                    return CreateSqliteContext();
                }
                else if (!string.IsNullOrEmpty(_userConnectionService.ConnectionString))
                {
                    return CreateMariaDbContext();
                }

                else
                {
                    throw new InvalidOperationException("Aucune configuration de base de données valide");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur création DbContext");

                if (!HybridSupport.IsElectronActive) throw;

                // Fallback SQLite
                return CreateSqliteContext();
            }
        }

        private ApplicationDbContext CreateMariaDbContext()
        {
            if (_cachedMariaDbOptions == null)
            {
                lock (_lockObject)
                {
                    if (_cachedMariaDbOptions == null)
                    {
                        // S'assurer que le tunnel est actif
                        _sshTunnelService.EnsureSshTunnel();

                        var connectionString = _userConnectionService.ConnectionString
                            .Replace("Server=localhost", "Server=127.0.0.1")
                            .Replace("Port=3306", $"Port={_sshTunnelService.LocalPort}");

                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        var serverVersion = new MariaDbServerVersion(new Version(10, 6, 21));

                        optionsBuilder.UseMySql(
                            connectionString,
                            serverVersion,
                            options =>
                            {
                                options.EnableRetryOnFailure(
                                    maxRetryCount: 3,
                                    maxRetryDelay: TimeSpan.FromSeconds(10),
                                    errorNumbersToAdd: null);
                                options.CommandTimeout(30);
                            })
                            .EnableSensitiveDataLogging(false)
                            .EnableServiceProviderCaching()
                            .EnableDetailedErrors(false);

                        _cachedMariaDbOptions = optionsBuilder.Options;
                    }
                }
            }

            return new ApplicationDbContext(_cachedMariaDbOptions);
        }

        private ApplicationDbContext CreateSqliteContext()
        {
            if (_cachedSqliteOptions == null)
            {
                lock (_lockObject)
                {
                    if (_cachedSqliteOptions == null)
                    {
                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
                        Directory.CreateDirectory(facilysPath);

                        var connectionString = $"Data Source={Path.Combine(facilysPath, "data.db")}";

                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlite(connectionString)
                            .EnableSensitiveDataLogging(false)
                            .EnableServiceProviderCaching()
                            .EnableDetailedErrors(false);

                        _cachedSqliteOptions = optionsBuilder.Options;
                    }
                }
            }

            return new ApplicationDbContext(_cachedSqliteOptions);
        }

        // Méthode pour invalider le cache en cas de changement de configuration
        public static void InvalidateCache()
        {
            lock (_lockObject)
            {
                _cachedMariaDbOptions = null;
                _cachedSqliteOptions = null;
            }
        }
    }
}