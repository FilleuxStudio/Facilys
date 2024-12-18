using Facilys.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Data
{
    public class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Vérifiez si la base de données contient déjà des utilisateurs
            if (context.Users.Any())
            {
                return; // La base de données a été initialisée
            }

            // Créez un utilisateur administrateur par défaut
            var adminUser = new Users
            {
                Lname = "Admin Local",
                Fname = "Administrateur",
                Email = "admin@facilys.fr",
                Login = "AdminLocal",
                Password = Users.HashPassword("Admin123456"), // Assurez-vous que le mot de passe respecte vos règles de sécurité
                Role = RoleUser.Administrator,
                DateAdded = DateTime.Now
            };

            // Créez un utilisateur concessionnaire par défaut
            var DealerUser = new Users
            {
                Lname = "Concessionnaire",
                Fname = "Concessionnaire",
                Email = "admin@facilys.fr",
                Login = "ConcessionnaireLocal",
                Password = Users.HashPassword("Dealer123456"), // Assurez-vous que le mot de passe respecte vos règles de sécurité
                Role = RoleUser.User,
                DateAdded = DateTime.Now
            };

            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}
