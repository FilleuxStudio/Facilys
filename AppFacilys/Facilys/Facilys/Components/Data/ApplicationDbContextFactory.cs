using Facilys.Components.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Renci.SshNet;
using System;
using System.IO;

namespace Facilys.Components.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private ForwardedPortLocal _port;
        private SshClient _sshClient;
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            var useMariaDb = args.Any(a => a.Equals("UseMariaDb", StringComparison.OrdinalIgnoreCase)) ||
                            Environment.GetEnvironmentVariable("FACILYS_USE_MARIADB") == "1";

            if (useMariaDb)
            {
                // SSH tunnel setup
                Console.WriteLine(">> Initialisation du tunnel SSH pour MariaDB...");

                var sshHost = Environment.GetEnvironmentVariable("SSH_HOST") ?? "ssh.example.com";
                var sshPort = int.Parse(Environment.GetEnvironmentVariable("SSH_PORT") ?? "22");
                var sshUser = Environment.GetEnvironmentVariable("SSH_USER") ?? "sshuser";
                var sshPass = Environment.GetEnvironmentVariable("SSH_PASS") ?? "sshpass";

                int localPort = new Random().Next(6000, 7000);

                _sshClient = new SshClient(sshHost, sshPort, sshUser, sshPass);
                _sshClient.Connect();

                _port = new ForwardedPortLocal("127.0.0.1", (uint)localPort, "127.0.0.1", 3306);
                _sshClient.AddForwardedPort(_port);
                _port.Start();

                if (!_sshClient.IsConnected || !_port.IsStarted)
                {
                    throw new Exception("Échec du tunnel SSH.");
                }

                Console.WriteLine($">> Tunnel actif sur le port local {localPort}");

                string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "jmaqmsnt_user_filleuxstudio";
                string dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? "p5evqQ/agsmNQ9";
                string dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "jmaqmsnt_user_filleuxstudio_db";

                string connStr = $"server=127.0.0.1;port={localPort};user={dbUser};password={dbPass};database={dbName}";
                optionsBuilder.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
            }
            else
            {
                // 🖥️ Mode Electron - SQLite local
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var facilysPath = Path.Combine(documentsPath, "Facilys", "Database");
                Directory.CreateDirectory(facilysPath);
                var connectionString = $"Data Source={Path.Combine(facilysPath, "data.db")}";

                optionsBuilder.UseSqlite(connectionString);
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
