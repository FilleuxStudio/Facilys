using Facilys.Components.Models;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Data
{
    public class DataSeeder
    {
        private readonly ApplicationDbContext _context;
        public DataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            await SeedClientssAsync();
        }

        private async Task SeedClientssAsync()
        {
            List<Clients> Clientss = new List<Clients>
            {
            new Clients { Id = Guid.NewGuid(), Lname = "Dupont", Fname = "Jean", Address = "123 Rue de la Paix", City = "Paris", PostalCode = "75001", Type = TypeClient.Client, AdditionalInformation = "Clients fidèle depuis 2010", DateCreated = DateTime.Parse("2025-02-05 21:00:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Martin", Fname = "Sophie", Address = "45 Avenue des Champs-Élysées", City = "Paris", PostalCode = "75008", Type = TypeClient.Client, AdditionalInformation = "Préfère le contact par email", DateCreated = DateTime.Parse("2025-02-05 21:01:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Dubois", Fname = "Pierre", Address = "78 Rue du Commerce", City = "Lyon", PostalCode = "69002", Type = TypeClient.ClientProfessional, AdditionalInformation = "Entreprise de plomberie", DateCreated = DateTime.Parse("2025-02-05 21:02:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Lefebvre", Fname = "Marie", Address = "15 Place Bellecour", City = "Lyon", PostalCode = "69002", Type = TypeClient.Client, AdditionalInformation = "Allergique aux arachides", DateCreated = DateTime.Parse("2025-02-05 21:03:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Moreau", Fname = "Luc", Address = "56 Rue de la République", City = "Marseille", PostalCode = "13001", Type = TypeClient.Client, AdditionalInformation = "Membre du programme de fidélité", DateCreated = DateTime.Parse("2025-02-05 21:04:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Roux", Fname = "Céline", Address = "23 Quai des Belges", City = "Marseille", PostalCode = "13001", Type = TypeClient.ClientProfessional, AdditionalInformation = "Boutique de vêtements", DateCreated = DateTime.Parse("2025-02-05 21:05:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Fournier", Fname = "Antoine", Address = "8 Rue du Vieux Marché", City = "Rouen", PostalCode = "76000", Type = TypeClient.Client, AdditionalInformation = "Intéressé par les nouveaux produits", DateCreated = DateTime.Parse("2025-02-05 21:06:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Vincent", Fname = "Isabelle", Address = "12 Place du Capitole", City = "Toulouse", PostalCode = "31000", Type = TypeClient.Client, AdditionalInformation = "Préfère être contactée le soir", DateCreated = DateTime.Parse("2025-02-05 21:07:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Mercier", Fname = "Thomas", Address = "34 Rue Nationale", City = "Lille", PostalCode = "59000", Type = TypeClient.ClientProfessional, AdditionalInformation = "Société de consulting", DateCreated = DateTime.Parse("2025-02-05 21:08:00") },
            new Clients { Id = Guid.NewGuid(), Lname = "Chevalier", Fname = "Emma", Address = "5 Cours Mirabeau", City = "Aix-en-Provence", PostalCode = "13100", Type = TypeClient.Client, AdditionalInformation = "Clientse VIP", DateCreated = DateTime.Parse("2025-02-05 21:09:00") }
            };

            foreach (var Client in Clientss)
            {
                if (!await _context.Clients.AnyAsync(c => c.Id == Client.Id))
                {
                    await _context.Clients.AddAsync(Client);
                }
            }

            await _context.SaveChangesAsync();

            if (Clientss != null)
            {
                var Vehicless = new List<Vehicles>
            {
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[0], Immatriculation = "AA-123-BB", Type = "Berline", Mark = "Renault", Model = "Clio", VIN = "VF1RB1N0H12345678", AdditionalInformation = "Véhicule principal", CirculationDate = DateTime.Parse("2020-06-15"), KM = 45000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:10:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[1], Immatriculation = "BB-456-CC", Type = "SUV", Mark = "Peugeot", Model = "3008", VIN = "VF3MRHNS0KS123456", AdditionalInformation = "Véhicule familial", CirculationDate = DateTime.Parse("2019-03-22"), KM = 62000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:11:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[2], Immatriculation = "CC-789-DD", Type = "Utilitaire", Mark = "Citroën", Model = "Berlingo", VIN = "VF7GBKFWC0J123456", AdditionalInformation = "Véhicule professionnel", CirculationDate = DateTime.Parse("2021-09-10"), KM = 30000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:12:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[3], Immatriculation = "DD-012-EE", Type = "Citadine", Mark = "Toyota", Model = "Yaris", VIN = "JTDKG3EA60N123456", AdditionalInformation = "Véhicule secondaire", CirculationDate = DateTime.Parse("2018-11-30"), KM = 75000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:13:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[4], Immatriculation = "EE-345-FF", Type = "Break", Mark = "Volkswagen", Model = "Passat", VIN = "WVWZZZ3CZLE123456", AdditionalInformation = "Véhicule de fonction", CirculationDate = DateTime.Parse("2022-01-05"), KM = 25000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:14:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[5], Immatriculation = "FF-678-GG", Type = "Fourgon", Mark = "Ford", Model = "Transit", VIN = "WF0XXXTTGXNU12345", AdditionalInformation = "Véhicule de livraison", CirculationDate = DateTime.Parse("2020-07-20"), KM = 50000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:15:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[6], Immatriculation = "GG-901-HH", Type = "Coupé", Mark = "BMW", Model = "Série 4", VIN = "WBA4J1C07LB123456", AdditionalInformation = "Véhicule de loisir", CirculationDate = DateTime.Parse("2023-03-15"), KM = 10000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:16:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[7], Immatriculation = "HH-234-II", Type = "Monospace", Mark = "Opel", Model = "Zafira", VIN = "W0LPE8EE8G6123456", AdditionalInformation = "Véhicule familial", CirculationDate = DateTime.Parse("2017-12-01"), KM = 90000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:17:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[8], Immatriculation = "II-567-JJ", Type = "Berline", Mark = "Audi", Model = "A4", VIN = "WAUZZZ8K9NA123456", AdditionalInformation = "Véhicule de représentation", CirculationDate = DateTime.Parse("2021-11-10"), KM = 35000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:18:00") },
            new Vehicles { Id = Guid.NewGuid(), Client = Clientss[9], Immatriculation = "JJ-890-KK", Type = "Cabriolet", Mark = "Mercedes", Model = "Classe C", VIN = "WDD2050421R123456", AdditionalInformation = "Véhicule de loisir", CirculationDate = DateTime.Parse("2022-06-30"), KM = 15000, StatusDataView = 0, DateAdded = DateTime.Parse("2025-02-05 21:19:00") }
            };

                foreach (var Vehicle in Vehicless)
                {
                    if (!await _context.Vehicles.AnyAsync(v => v.Id == Vehicle.Id))
                    {
                        await _context.Vehicles.AddAsync(Vehicle);
                    }
                }

                await _context.SaveChangesAsync();

                var otherVehicless = new List<OtherVehicles>
            {
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[0], SerialNumber = "SN12345", Type = "Moto", Mark = "Yamaha", Model = "MT-07", StatusDataView = 0, AdditionalInformation = "Moto de loisir", DateAdded = DateTime.Parse("2025-02-05 21:20:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[1], SerialNumber = "SN23456", Type = "Scooter", Mark = "Piaggio", Model = "Vespa", StatusDataView = 0, AdditionalInformation = "Scooter urbain", DateAdded = DateTime.Parse("2025-02-05 21:21:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[2], SerialNumber = "SN34567", Type = "Vélo électrique", Mark = "Bosch", Model = "City", StatusDataView = 0, AdditionalInformation = "Vélo pour déplacements professionnels", DateAdded = DateTime.Parse("2025-02-05 21:22:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[3], SerialNumber = "SN45678", Type = "Trottinette électrique", Mark = "Xiaomi", Model = "Pro 2", StatusDataView = 0, AdditionalInformation = "Trottinette pour courts trajets", DateAdded = DateTime.Parse("2025-02-05 21:23:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[4], SerialNumber = "SN56789", Type = "Quad", Mark = "Yamaha", Model = "Grizzly", StatusDataView = 0, AdditionalInformation = "Quad pour loisirs", DateAdded = DateTime.Parse("2025-02-05 21:24:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[5], SerialNumber = "SN67890", Type = "Remorque", Mark = "Lider", Model = "Robust", StatusDataView = 0, AdditionalInformation = "Remorque pour transport de marchandises", DateAdded = DateTime.Parse("2025-02-05 21:25:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[6], SerialNumber = "SN78901", Type = "Jet Ski", Mark = "Kawasaki", Model = "Ultra 310LX", StatusDataView = 0, AdditionalInformation = "Jet Ski pour loisirs nautiques", DateAdded = DateTime.Parse("2025-02-05 21:26:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[7], SerialNumber = "SN89012", Type = "Camping-car", Mark = "Fiat", Model = "Ducato", StatusDataView = 0, AdditionalInformation = "Camping-car pour vacances", DateAdded = DateTime.Parse("2025-02-05 21:27:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[8], SerialNumber = "SN90123", Type = "Bateau", Mark = "Bénéteau", Model = "Flyer 8", StatusDataView = 0, AdditionalInformation = "Bateau de plaisance", DateAdded = DateTime.Parse("2025-02-05 21:28:00") },
            new OtherVehicles { Id = Guid.NewGuid(), Client = Clientss[9], SerialNumber = "SN01234", Type = "Vélo", Mark = "Trek", Model = "FX 3 Disc", StatusDataView = 0, AdditionalInformation = "Vélo de ville", DateAdded = DateTime.Parse("2025-02-05 21:29:00") }
            };

                foreach (var otherVehicles in otherVehicless)
                {
                    if (!await _context.OtherVehicles.AnyAsync(ov => ov.Id == otherVehicles.Id))
                    {
                        await _context.OtherVehicles.AddAsync(otherVehicles);
                    }
                }

                await _context.SaveChangesAsync();

                var emails = new List<EmailsClients>
        {
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[0], Email = "jean.dupont@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[1], Email = "sophie.martin@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[2], Email = "pierre.dubois@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[3], Email = "marie.lefebvre@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[4], Email = "luc.moreau@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[5], Email = "celine.roux@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[6], Email = "antoine.fournier@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[7], Email = "isabelle.vincent@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[8], Email = "thomas.mercier@email.com" },
            new EmailsClients { Id = Guid.NewGuid(), Client = Clientss[9], Email = "emma.chevalier@email.com" }
            };

                foreach (var email in emails)
                {
                    if (!await _context.Emails.AnyAsync(e => e.Id == email.Id))
                    {
                        await _context.Emails.AddAsync(email);
                    }
                }

                await _context.SaveChangesAsync();


                var phones = new List<PhonesClients>
        {
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[0], Phone = "0123456789" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[1], Phone = "0234567890" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[2], Phone = "0345678901" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[3], Phone = "0456789012" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[4], Phone = "0567890123" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[5], Phone = "0678901234" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[6], Phone = "0789012345" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[7], Phone = "0890123456" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[8], Phone = "0901234567" },
            new PhonesClients { Id = Guid.NewGuid(), Client = Clientss[9], Phone = "0012345678" }
            };

                foreach (var phone in phones)
                {
                    if (!await _context.Phones.AnyAsync(p => p.Id == phone.Id))
                    {
                        await _context.Phones.AddAsync(phone);
                    }
                }

                await _context.SaveChangesAsync();
            }

        }
    }
}
