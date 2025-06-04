using ElectronNET.API;
using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class ManagerClients
    {
        readonly List<ManagerClienViewtList> managerClienViewtLists = [];
        readonly ManagerClientViewModel managerClientViewModel = new();
        readonly ModalManagerId modalManager = new();
        ApplicationDbContext DbContext;
        Guid selectClient = Guid.Empty;

        private string currentPhone = string.Empty, currentEmail = string.Empty;

        private void Init()
        {
            managerClientViewModel.Client = new();
            managerClientViewModel.VehicleClient = new();
            managerClientViewModel.PhonesClients = [];
            managerClientViewModel.EmailsClients = [];
            managerClientViewModel.Vehicles = [];
        }

        protected override async Task OnInitializedAsync()
        {
            //await InvokeAsync(() =>
            //{
            //    PageTitleService.CurrentTitle = "Gestion clients";
            //});

            PageTitleService.CurrentTitle = "Gestion clients";

            Init();

            modalManager.RegisterModal("OpenModalLargeAddClient");
            modalManager.RegisterModal("OpenModalLargeEditClient");
            modalManager.RegisterModal("OpenModalLargeDeleteClient");
            modalManager.RegisterModal("OpenModalLargeAddVehicle");
            modalManager.RegisterModal("OpenModaInfoVehicles");

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UserConnection.LoadCredentialsAsync();
                await LoadDataHeader();
                //await JSRuntime.InvokeVoidAsync("loadScript", "/assets/libs/simple-datatables/umd/simple-datatables.js");
                StateHasChanged(); // Demande un nouveau rendu du composant
            }
        }

        private async Task LoadDataHeader()
        {
            DbContext = await DbContextFactory.CreateDbContextAsync();

            managerClientViewModel.EmailsClients = await DbContext.Emails.Include(c => c.Client).ToListAsync();
            managerClientViewModel.PhonesClients = await DbContext.Phones.Include(c => c.Client).ToListAsync();
            managerClientViewModel.Vehicles = await DbContext.Vehicles.Include(c => c.Client).ToListAsync();
            var clients = await DbContext.Clients.ToListAsync();

            foreach (var client in clients)
            {
                managerClienViewtLists.Add(new()
                {
                    Client = client,
                    EmailsClients = [.. managerClientViewModel.EmailsClients.Where(c => c.Client.Id == client.Id)],
                    PhonesClients = [.. managerClientViewModel.PhonesClients.Where(c => c.Client.Id == client.Id)],
                    Vehicles = [.. managerClientViewModel.Vehicles.Where(c => c.Client.Id == client.Id)],
                });
            }

        }

        private static Task OpenGoogleMaps(string clientIndex)
        {
            var url = "https://www.google.com/maps";

            if (HybridSupport.IsElectronActive)
            {
                // Mode Electron (application desktop)
                return ElectronNET.API.Electron.Shell.OpenExternalAsync(url);
            }
            else
            {
                // Mode Web (Cloud Run) - Ouvre dans un nouvel onglet
                return Task.FromResult(() =>
                {
                    // Ce code sera exécuté côté client via JavaScript
                    Console.WriteLine($"Redirection JS vers: {url}");
                });
            }
        }

        /// <summary>
        /// Ouvre un modal sans données supplémentaires.
        /// </summary>
        /// <param name="id">L'identifiant du modal à ouvrir.</param>
        private async void OpenModal(string id)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            Init();
            StateHasChanged();
        }

        /// <summary>
        /// Ouvre un modal avec des données spécifiques à un utilisateur.
        /// </summary>
        /// <param name="idModal">L'identifiant du modal à ouvrir.</param>
        /// <param name="idClient">L'identifiant de l'utilisateur dont les données doivent être chargées.</param>
        private async void OpenModalData(string idModal, Guid idClient)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(idModal);
            managerClientViewModel.Client = await DbContext.Clients.Where(i => i.Id == idClient).FirstOrDefaultAsync();
            managerClientViewModel.EmailsClients = await DbContext.Emails.Include(c => c.Client).Where(u => u.Client.Id == idClient).ToListAsync();
            managerClientViewModel.PhonesClients = await DbContext.Phones.Include(c => c.Client).Where(u => u.Client.Id == idClient).ToListAsync();
            StateHasChanged();
        }

        /// <summary>
        /// Ouvre un modal avec les données véhicules associé à un utilisateur.
        /// </summary>
        /// <param name="idModal"></param>
        /// <param name="idClient"></param>
        private async void OpenModalDataVehicle(string idModal, Guid idClient)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(idModal);
            managerClientViewModel.Client = await DbContext.Clients.Where(i => i.Id == idClient).FirstOrDefaultAsync();
            managerClientViewModel.Vehicles = await DbContext.Vehicles.Include(c => c.Client).Where(u => u.Client.Id == idClient).ToListAsync();
            StateHasChanged();
        }

        /// <summary>
        /// Fermeture du modal
        /// </summary>
        /// <param name="idModal"></param>
        private async void CloseModal(string idModal)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", false);
            modalManager.CloseModal(idModal);
            ResetForm();
            StateHasChanged();
        }

        private void HandleKeyUpPhone(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" || e.Key == ";")
            {
                AddPhone();
            }
        }

        private void HandleKeyDownPhone(KeyboardEventArgs e)
        {
            if (e.Key == "Delete")
            {
                RemoveSelectedPhones();
            }
        }

        private void HandleKeyUpEmail(KeyboardEventArgs e)
        {

            if (e.Key == "Enter" || e.Key == ";")
            {
                AddEmail();
            }
        }

        private void HandleKeyDownEmail(KeyboardEventArgs e)
        {
            if (e.Key == "Delete")
            {
                RemoveSelectedEmails();
            }
        }

        private void AddPhone()
        {
            if (!string.IsNullOrWhiteSpace(currentPhone))
            {
                managerClientViewModel.PhonesClients.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Phone = currentPhone.Trim(),
                });
                currentPhone = "";
            }
        }

        private void AddEmail()
        {
            if (!string.IsNullOrWhiteSpace(currentEmail))
            {
                managerClientViewModel.EmailsClients.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Email = currentEmail.Trim(),
                });
                currentEmail = "";
            }
        }


        private void HandleEmailSelection(ChangeEventArgs e)
        {
            string[] selectedEmailAddress = (string[])e.Value;
            managerClientViewModel.EmailsClient = managerClientViewModel.EmailsClients
                .FirstOrDefault(ec => ec.Email == selectedEmailAddress[0]);
        }

        private void HandlePhoneSelection(ChangeEventArgs e)
        {
            string[] selectedPhone = (string[])e.Value;
            managerClientViewModel.PhonesClient = managerClientViewModel.PhonesClients
                .FirstOrDefault(ec => ec.Phone == selectedPhone[0]);
        }

        private void RemoveSelectedEmails()
        {
            if (managerClientViewModel.EmailsClient != null)
            {
                managerClientViewModel.EmailsClients.Remove(managerClientViewModel.EmailsClient);
                managerClientViewModel.EmailsClient = null;
            }

            StateHasChanged();
        }

        private void RemoveSelectedPhones()
        {
            if (managerClientViewModel.PhonesClient != null)
            {
                managerClientViewModel.PhonesClients.Remove(managerClientViewModel.PhonesClient);
                managerClientViewModel.PhonesClient = null;
            }
            StateHasChanged();
        }

        private async Task SubmitAddClient()
        {
            if (managerClientViewModel.Client.Fname != "" && managerClientViewModel.Client.Lname != "" && managerClientViewModel.Client.Address != "")
            {
                try
                {
                    managerClientViewModel.Client.Id = Guid.NewGuid();
                    managerClientViewModel.Client.DateCreated = DateTime.Now;
                    await DbContext.Clients.AddAsync(managerClientViewModel.Client);
                    await DbContext.SaveChangesAsync();

                    if (managerClientViewModel.PhonesClients.Count != 0)
                    {
                        for (int i = 0; i < managerClientViewModel.PhonesClients.Count; i++)
                        {
                            managerClientViewModel.PhonesClients[i].Client = managerClientViewModel.Client;
                        }

                        await DbContext.Phones.AddRangeAsync(managerClientViewModel.PhonesClients);
                    }

                    if (managerClientViewModel.EmailsClients.Count != 0)
                    {
                        for (int i = 0; i < managerClientViewModel.EmailsClients.Count; i++)
                        {
                            managerClientViewModel.EmailsClients[i].Client = managerClientViewModel.Client;
                        }
                        await DbContext.Emails.AddRangeAsync(managerClientViewModel.EmailsClients);
                    }

                    await DbContext.SaveChangesAsync();

                    if (HybridSupport.IsElectronActive)
                    {
                        // Envoi vers l'API Node.js avec les DTOs
                        if (HybridSupport.IsElectronActive)
                        {
                            var clientDto = managerClientViewModel.Client.ToDto();
                            var phonesDto = managerClientViewModel.PhonesClients.Select(p => p.ToDto()).ToArray();
                            var emailsDto = managerClientViewModel.EmailsClients.Select(e => e.ToDto()).ToArray();

                            await SyncService.PushChangesAsync("api/query/addclient", new[] { clientDto });
                            await SyncService.PushChangesAsync("api/query/addphone", phonesDto);
                            await SyncService.PushChangesAsync("api/query/addemail", emailsDto);
                        }
                    }

                    ResetForm();

                    CloseModal("OpenModalLargeAddClient");

                    await RefreshClientList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
                }
            }
        }

        private async void AddCustomerAndVehicle()
        {
            await SubmitAddClient();

            managerClientViewModel.VehicleClient = new()
            {
                Client = await DbContext.Clients.OrderByDescending(c => c.DateCreated).FirstOrDefaultAsync()
            };
            selectClient = managerClientViewModel.VehicleClient.Client.Id;
            OpenModal("OpenModalLargeAddVehicle");
        }
        private async Task SubmitEditClient()
        {
            try
            {
                // Création de la stratégie d'exécution pour MariaDB
                var executionStrategy = DbContext.Database.CreateExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await DbContext.Database.BeginTransactionAsync();

                    // Mise à jour du client principal
                    DbContext.Clients.Update(managerClientViewModel.Client);
                    await DbContext.SaveChangesAsync();

                    // Suppression des relations existantes
                    await DbContext.Database.ExecuteSqlRawAsync(
                        "DELETE FROM Phones WHERE IdClient = {0}",
                        managerClientViewModel.Client.Id
                    );

                    await DbContext.Database.ExecuteSqlRawAsync(
                        "DELETE FROM Emails WHERE IdClient = {0}",
                        managerClientViewModel.Client.Id
                    );

                    // Ajout des nouveaux téléphones
                    if (managerClientViewModel.PhonesClients?.Count > 0)
                    {
                        foreach (var phone in managerClientViewModel.PhonesClients)
                        {
                            phone.Client.Id = managerClientViewModel.Client.Id;
                        }
                        await DbContext.Phones.AddRangeAsync(managerClientViewModel.PhonesClients);
                    }

                    // Ajout des nouveaux emails
                    if (managerClientViewModel.EmailsClients?.Count > 0)
                    {
                        foreach (var email in managerClientViewModel.EmailsClients)
                        {
                            email.Client.Id = managerClientViewModel.Client.Id;
                        }
                        await DbContext.Emails.AddRangeAsync(managerClientViewModel.EmailsClients);
                    }

                    // Validation finale
                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    if (HybridSupport.IsElectronActive)
                    {
                        // Envoi vers l'API Node.js avec les DTOs
                        if (HybridSupport.IsElectronActive)
                        {
                            var clientDto = managerClientViewModel.Client.ToDto();
                            var phonesDto = managerClientViewModel.PhonesClients.Select(p => p.ToDto()).ToArray();
                            var emailsDto = managerClientViewModel.EmailsClients.Select(e => e.ToDto()).ToArray();

                            await SyncService.PushChangesAsync("api/query/updateclient", new[] { clientDto });
                            await SyncService.PushChangesAsync("api/query/addphone", phonesDto);
                            await SyncService.PushChangesAsync("api/query/addemail", emailsDto);
                        }
                    }

                    ResetForm();
                    CloseModal("OpenModalLargeEditClient");
                    await RefreshClientList();
                });
             }
            catch (Exception ex)
            {
                Logger.LogError(message: ex.Message, "Erreur lors de la mise à jour de la base de données");
            }
        }

        private async Task SubmitDeleteClientAllData()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {

                var clientId = managerClientViewModel.Client.Id;

                // Supprimer les téléphones et emails
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Phones WHERE IdClient = {0}", clientId);
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Emails WHERE IdClient = {0}", clientId);

                // Supprimer les devis et leurs éléments
                var quotes = await DbContext.Quotes.Where(q => q.Client.Id == clientId).ToListAsync();
                foreach (var quote in quotes)
                {
                    await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM QuotesItems WHERE IdQuote = {0}", quote.Id);
                }
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Quotes WHERE IdClient = {0}", clientId);

                // Supprimer les véhicules et autres véhicules
                var vehicles = await DbContext.Vehicles.Where(v => v.Client.Id == clientId).ToListAsync();
                var otherVehicles = await DbContext.OtherVehicles.Where(ov => ov.Client.Id == clientId).ToListAsync();

                // Supprimer les factures et l'historique des pièces liés aux véhicules
                foreach (var vehicle in vehicles)
                {
                    await DeleteInvoicesAndHistoryForVehicle(vehicle.Id, isOtherVehicle: false);
                }

                foreach (var otherVehicle in otherVehicles)
                {
                    await DeleteInvoicesAndHistoryForVehicle(otherVehicle.Id, isOtherVehicle: true);
                }

                // Supprimer les véhicules et autres véhicules
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Vehicles WHERE IdClient = {0}", clientId);
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM OtherVehicles WHERE IdClient = {0}", clientId);

                // Supprimer le client
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Clients WHERE Id = {0}", clientId);

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Réinitialiser le formulaire et rafraîchir la liste des clients
                ResetForm();
                CloseModal("OpenModalLargeDeleteClient");
                await RefreshClientList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la suppréssion des données client");
            }
        }

        private async Task SubmitDeleteClient()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                var clientId = managerClientViewModel.Client.Id;

                // Créer ou récupérer l'utilisateur concessionnaire par défaut
                var dealerClient = await DbContext.Clients.FirstOrDefaultAsync(c => c.Lname == "ConcessionnaireLock" && c.Fname == "Concessionnaire") ?? new Clients
                {
                    Lname = "ConcessionnaireLock",
                    Fname = "Concessionnaire",
                    Address = "local",
                    City = "local",
                    PostalCode = "60840",
                    Type = TypeClient.Client,
                    AdditionalInformation = "Client concessionnaire par défaut",
                    DateCreated = DateTime.Now
                };

                if (dealerClient.Id == Guid.Empty)
                {
                    DbContext.Clients.Add(dealerClient);
                    await DbContext.SaveChangesAsync();
                }

                await DbContext.Database.ExecuteSqlRawAsync("UPDATE Vehicles SET IdClient = {0} WHERE IdClient = {1}", dealerClient.Id, clientId);
                await DbContext.Database.ExecuteSqlRawAsync("UPDATE OtherVehicles SET IdClient = {0} WHERE IdClient = {1}", dealerClient.Id, clientId);

                // Supprimer les devis et leurs éléments
                var quotes = await DbContext.Quotes.Where(q => q.Client.Id == clientId).ToListAsync();
                foreach (var quote in quotes)
                {
                    await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM QuotesItems WHERE IdQuote = {0}", quote.Id);
                }
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Quotes WHERE IdClient = {0}", clientId);
                // Supprimer les téléphones et emails
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Phones WHERE IdClient = {0}", clientId);
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Emails WHERE IdClient = {0}", clientId);

                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Supprimer l'ancien client
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Clients WHERE Id = {0}", clientId);

                await DbContext.SaveChangesAsync();


                // Réinitialiser le formulaire et rafraîchir la liste des clients
                ResetForm();
                CloseModal("OpenModalLargeDeleteClient");
                await RefreshClientList();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Gérer l'exception (par exemple, journalisation, affichage d'un message d'erreur)
                Console.WriteLine($"Une erreur s'est produite lors du remplacement du client : {ex.Message}");
            }
        }

        private async Task SubmitAddVehicleStepTow()
        {
            try
            {
                managerClientViewModel.VehicleClient.Id = Guid.NewGuid();
                managerClientViewModel.VehicleClient.DateAdded = DateTime.Now;
                managerClientViewModel.VehicleClient.Client = await DbContext.Clients.FindAsync(selectClient);
                await DbContext.Vehicles.AddAsync(managerClientViewModel.VehicleClient);
                await DbContext.SaveChangesAsync();

                ResetForm();

                CloseModal("OpenModalLargeAddVehicle");

                await RefreshClientList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
            }
        }


        // Méthode helper pour supprimer les factures et l'historique
        private async Task DeleteInvoicesAndHistoryForVehicle(Guid vehicleId, bool isOtherVehicle)
        {
            var invoices = isOtherVehicle
                ? await DbContext.Invoices.Where(i => i.OtherVehicle.Id == vehicleId).ToListAsync()
                : await DbContext.Invoices.Where(i => i.Vehicle.Id == vehicleId).ToListAsync();

            foreach (var invoice in invoices)
            {
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM HistoryPart WHERE IdInvoice = {0}", invoice.Id);
            }

            if (isOtherVehicle)
            {
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Invoices WHERE IdOtherVehicle = {0}", vehicleId);
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM MaintenanceAlerts WHERE IdOtherVehicle = {0}", vehicleId);
            }
            else
            {
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Invoices WHERE IdVehicle = {0}", vehicleId);
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM MaintenanceAlerts WHERE IdVehicle = {0}", vehicleId);
            }
        }
        private void ResetForm()
        {
            managerClientViewModel.Client = new(); // Réinitialisez avec un nouvel objet Client
            managerClientViewModel.PhonesClients.Clear();
            managerClientViewModel.PhonesClients = [];
            managerClientViewModel.EmailsClients.Clear();
            managerClientViewModel.EmailsClients = [];
            currentPhone = string.Empty;
            currentEmail = string.Empty;
        }

        private async Task RefreshClientList()
        {
            // Récupérer la liste mise à jour des clients depuis votre service
            managerClienViewtLists.Clear();
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
            // await InvokeAsync(StateHasChanged);
            // StateHasChanged();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
