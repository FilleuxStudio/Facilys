using Facilys.Components.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Facilys.Components.Data
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<VersionDatabase> VersionDatabases { get; set; }
        public DbSet<SyncMetaData> SyncMetaDatas { get; set; }
        public DbSet<Clients> Clients { get; set; }
        public DbSet<EmailsClients> Emails { get; set; }
        public DbSet<PhonesClients> Phones { get; set; }
        public DbSet<ProfessionalCustomer> ProfessionalClient { get; set; }
        public DbSet<Vehicles> Vehicles { get; set; }
        public DbSet<OtherVehicles> OtherVehicles { get; set; }
        public DbSet<MaintenanceAlert> MaintenanceAlerts { get; set; }
        public DbSet<Invoices> Invoices { get; set; }
        public DbSet<HistoryPart> HistoryParts { get; set; }
        public DbSet<Inventorys> Inventorys { get; set; }
        public DbSet<Quotes> Quotes { get; set; }
        public DbSet<QuotesItems> QuotesItems { get; set; }
        public DbSet<EditionSetting> EditionSettings { get; set; }
        public DbSet<ReferencesIgnored> ReferencesIgnored { get; set; }
        public DbSet<InterestingReferences> InterestingReferences {  get; set; }
        public DbSet<AssociationSettingReference> AssociationSettingReferences { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }

        //Commande
        /*
        dotnet ef migrations add AddAllModels
            dotnet ef database update*/


    }
}
