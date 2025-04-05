using ElectronNET.API;
using Facilys.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Facilys.Components.Data
{
    public class DynamicDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private IServiceProvider _provider;

        public DynamicDbContextFactory(IServiceProvider provider)
        {
            this._provider = provider;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var scope = _provider.CreateScope();
            var userConnection = scope.ServiceProvider.GetRequiredService<UserConnectionService>();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (!string.IsNullOrEmpty(userConnection.ConnectionString))
            {
                // Mode connecté - MariaDB utilisateur
                optionsBuilder.UseMySql(
                    userConnection.ConnectionString,
                    ServerVersion.AutoDetect(userConnection.ConnectionString));
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
    }
}
