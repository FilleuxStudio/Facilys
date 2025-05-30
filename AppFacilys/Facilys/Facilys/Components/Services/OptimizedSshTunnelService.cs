using Renci.SshNet;
using System.Collections.Concurrent;

namespace Facilys.Components.Services
{
    public class OptimizedSshTunnelService : IHostedService, IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OptimizedSshTunnelService> _logger;
        private SshClient _sshClient;
        private ForwardedPortLocal _forwardedPort;
        private readonly object _lockObject = new object();
        private bool _isInitialized = false;

        // Port fixe pour éviter les conflits
        public int LocalPort { get; private set; } = 5432;

        public OptimizedSshTunnelService(IConfiguration configuration, ILogger<OptimizedSshTunnelService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => EnsureSshTunnel(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void EnsureSshTunnel()
        {
            if (_isInitialized && IsTunnelActive) return;

            lock (_lockObject)
            {
                if (_isInitialized && IsTunnelActive) return;

                try
                {
                    // Nettoyage des anciennes connexions
                    CleanupTunnel();

                    var sshHost = _configuration["Ssh:Host"];
                    var sshPort = int.Parse(_configuration["Ssh:Port"] ?? "22");
                    var sshUser = _configuration["Ssh:Username"];
                    var sshPassword = _configuration["Ssh:Password"];

                    _logger.LogInformation("Établissement du tunnel SSH vers {Host}:{Port}", sshHost, sshPort);

                    // Configuration SSH optimisée
                    _sshClient = new SshClient(new Renci.SshNet.ConnectionInfo(sshHost, sshPort, sshUser,
                        new PasswordAuthenticationMethod(sshUser, sshPassword))
                    {
                        Timeout = TimeSpan.FromSeconds(30),
                    });

                    _sshClient.Connect();

                    // Port forwarding avec retry logic
                    var maxRetries = 3;
                    for (int i = 0; i < maxRetries; i++)
                    {
                        try
                        {
                            _forwardedPort = new ForwardedPortLocal("127.0.0.1", (uint)LocalPort, "127.0.0.1", 3306);
                            _sshClient.AddForwardedPort(_forwardedPort);
                            _forwardedPort.Start();
                            break;
                        }
                        catch (Exception ex) when (i < maxRetries - 1)
                        {
                            _logger.LogWarning("Tentative {Attempt} échouée, retry... {Error}", i + 1, ex.Message);
                            LocalPort++; // Essai sur port suivant
                            Thread.Sleep(1000);
                        }
                    }

                    if (!IsTunnelActive)
                    {
                        throw new Exception("Impossible d'établir le tunnel SSH après plusieurs tentatives");
                    }

                    _logger.LogInformation("Tunnel SSH actif sur port local {Port}", LocalPort);
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de l'établissement du tunnel SSH");
                    throw;
                }
            }
        }

        private void CleanupTunnel()
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
                _logger.LogWarning(ex, "Erreur lors du nettoyage du tunnel");
            }
            finally
            {
                _forwardedPort = null;
                _sshClient = null;
            }
        }

        public bool IsTunnelActive => _sshClient?.IsConnected == true && _forwardedPort?.IsStarted == true;

        public void Dispose()
        {
            CleanupTunnel();
            _isInitialized = false;
        }
    }
}