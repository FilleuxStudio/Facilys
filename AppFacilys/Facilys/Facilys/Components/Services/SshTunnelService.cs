using Renci.SshNet;

namespace Facilys.Components.Services
{
    public class SshTunnelService : IHostedService, IDisposable
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Le tunnel sera créé à la demande par DynamicDbContextFactory
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
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
                Console.WriteLine($"Erreur lors de la fermeture du tunnel SSH: {ex.Message}");
            }
        }

        public void EnsureSshTunnel()
        {
            if (_sshClient == null || !_sshClient.IsConnected)
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
        }

        public bool IsTunnelActive => _sshClient?.IsConnected == true && _forwardedPort?.IsStarted == true;

    }
}
