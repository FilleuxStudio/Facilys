﻿using ElectronNET.API;
using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Facilys.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class ManagerVehicles
    {
        readonly List<ManagerVehicleViewList> managerVehicleViewLists = [];
        readonly List<ManagerOtherVehicleViewList> managerOtherVehicleViewLists = [];
        readonly ManagerVehicleViewModel managerVehicleViewModel = new();
        readonly ModalManagerId modalManager = new();
        ApplicationDbContext DbContext;
        VINInfo VinInfo = new();
        Guid selectClient = Guid.Empty;
        [Inject] private EnvironmentApp EnvApp { get; set; }
        VehicleRegistrationDocumentAnalyzer documentAnalyzer;
        bool ViewRawOCRData = false;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion vehicules";
            });

            managerVehicleViewModel.Client = new();
            managerVehicleViewModel.Vehicle = new();
            managerVehicleViewModel.OtherVehicle = new();
            managerVehicleViewModel.Invoices = new();
            managerVehicleViewModel.HistoryPart = new();
            managerVehicleViewModel.Clients = [];

            modalManager.RegisterModal("OpenModalLargeAddVehicle");
            modalManager.RegisterModal("OpenModaSmallInfoVin");
            modalManager.RegisterModal("OpenModalLargeEditVehicle");
            modalManager.RegisterModal("OpenModalLargeDeleteVehicle");
            modalManager.RegisterModal("OpenModalLargeAddOtherVehicle");
            modalManager.RegisterModal("OpenModalLargeEditOtherVehicle");
            modalManager.RegisterModal("OpenModalLargeDeleteOtherVehicle");
            modalManager.RegisterModal("OpenModalDataOCR");

            documentAnalyzer = new(EnvApp);
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UserConnection.LoadCredentialsAsync();
                await LoadDataHeader();
                StateHasChanged(); // Demande un nouveau rendu du composant
            }
        }

        private async Task LoadDataHeader()
        {
            DbContext = await DbContextFactory.CreateDbContextAsync();
            var vehicles = await DbContext.Vehicles.Include(c => c.Client).Where(v => v.StatusDataView != StatusData.Delete).ToListAsync();
            foreach (var vehicle in vehicles)
            {
                managerVehicleViewLists.Add(new()
                {
                    Vehicle = vehicle,
                    Client = vehicle.Client,
                    Invoices = await DbContext.Invoices.Include(v => v.Vehicle).Where(h => h.Vehicle.Id == vehicle.Id).ToListAsync(),
                    // HistoryParts = await DbContext.HistoryParts.Include(v => v.Vehicle).Where(h => h.Vehicle.Id == vehicle.Id).ToListAsync(),
                });
            }

            var OtherVehicles = await DbContext.OtherVehicles.Include(c => c.Client).Where(v => v.StatusDataView != StatusData.Delete).ToListAsync();
            foreach (var vehicle in OtherVehicles)
            {
                managerOtherVehicleViewLists.Add(new()
                {
                    OtherVehicle = vehicle,
                    Client = vehicle.Client,
                    Invoices = await DbContext.Invoices.Include(v => v.OtherVehicle).Where(h => h.OtherVehicle.Id == vehicle.Id).ToListAsync(),
                    //HistoryParts = await DbContext.HistoryParts.Include(v => v.OtherVehicle).Where(h => h.OtherVehicle.Id == vehicle.Id).ToListAsync(),
                });
            }

            managerVehicleViewModel.Clients = await DbContext.Clients.ToListAsync();
        }

        private async void OpenModal(string id)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            StateHasChanged();
        }

        private async void OpenModalData(string id, Guid idVehicle)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            managerVehicleViewModel.Vehicle = await DbContext.Vehicles.Include(c => c.Client).Where(v => v.Id == idVehicle).FirstOrDefaultAsync();
            managerVehicleViewModel.OtherVehicle = await DbContext.OtherVehicles.Include(c => c.Client).Where(v => v.Id == idVehicle).FirstOrDefaultAsync();
            if (managerVehicleViewModel.Vehicle != null)
            {
                managerVehicleViewModel.Client = await DbContext.Clients.FindAsync(managerVehicleViewModel.Vehicle.Client.Id);
                managerVehicleViewModel.OtherVehicle = new();
            }
            else
            {
                managerVehicleViewModel.Client = await DbContext.Clients.FindAsync(managerVehicleViewModel.OtherVehicle.Client.Id);
                managerVehicleViewModel.Vehicle = new();
            }

            selectClient = managerVehicleViewModel.Client.Id;
            StateHasChanged();
        }

        private async void OpenModalData(string id, string vin)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            VINDecoderService decoderService = new();
            VinInfo = decoderService.DecodeVIN(vin);
            managerVehicleViewModel.Vehicle = await DbContext.Vehicles.Include(c => c.Client).Where(v => v.VIN == vin).FirstOrDefaultAsync();
            StateHasChanged();
        }

        private async void CloseModal(string id)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", false);
            modalManager.CloseModal(id);
            ResetForm();
            StateHasChanged();
        }

        private async void CloseModalOCR(string id)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", false);
            modalManager.CloseModal(id);
            StateHasChanged();
        }

        private void ResetForm()
        {
            managerVehicleViewModel.Client = new();
            managerVehicleViewModel.Vehicle = new();
            managerVehicleViewModel.OtherVehicle = new();
            managerVehicleViewModel.HistoryPart = new();
            ViewRawOCRData = false;
        }

        private async Task RefreshVehicleList()
        {
            // Récupérer la liste mise à jour des clients depuis votre service
            managerVehicleViewLists.Clear();
            managerOtherVehicleViewLists.Clear();
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
            // await InvokeAsync(StateHasChanged);
            // StateHasChanged();
        }

        private async Task SubmitAddVehicle()
        {
            try
            {
                managerVehicleViewModel.Vehicle.Id = Guid.NewGuid();
                managerVehicleViewModel.Vehicle.DateAdded = DateTime.Now;
                managerVehicleViewModel.Vehicle.Client = null;
                managerVehicleViewModel.Vehicle.IdClient = selectClient;
                await DbContext.Vehicles.AddAsync(managerVehicleViewModel.Vehicle);
                await DbContext.SaveChangesAsync();

                if (HybridSupport.IsElectronActive)
                {
                    // Envoi vers l'API Node.js avec les DTOs
                    if (HybridSupport.IsElectronActive)
                    {
                        var vehicleDto = managerVehicleViewModel.Vehicle.ToDto();

                        await SyncService.PushChangesAsync("api/query/addvehicle", new[] { vehicleDto });
                    }
                }

                ResetForm();

                CloseModal("OpenModalLargeAddVehicle");

                await RefreshVehicleList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
            }
        }

        private async Task SubmitEditVehicle()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {
                    DbContext.Vehicles.Update(managerVehicleViewModel.Vehicle);
                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Actions post-transaction (en dehors de la transaction)
                    ResetForm();
                    CloseModal("OpenModalLargeEditVehicle");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Logger.LogError(ex, "Erreur lors de la mise à jour de la base de données");
                    throw; // Re-lancer pour permettre la gestion d'erreur externe
                }
            });

            //await RefreshVehicleList();
        }

        private async Task SubmitDeleteVehicleAllData()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {

                    var vehicleId = managerVehicleViewModel.Vehicle.Id;

                    // Supprimer les factures et l'historique des pièces liés aux véhicules
                    await DeleteInvoicesAndHistoryForVehicle(vehicleId, isOtherVehicle: false);
                    await DeleteInvoicesAndHistoryForVehicle(vehicleId, isOtherVehicle: true);

                    DbContext.Vehicles.Remove(managerVehicleViewModel.Vehicle);

                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Réinitialiser le formulaire et rafraîchir la liste des clients
                    ResetForm();
                    CloseModal("OpenModalLargeDeleteVehicle");

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de la suppréssion des données véhicule");
                }
            });
            await RefreshVehicleList();
        }

        private async Task SubmitDeleteVehicle()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {
                    managerVehicleViewModel.Vehicle.StatusDataView = StatusData.Delete;
                    DbContext.Vehicles.Update(managerVehicleViewModel.Vehicle);
                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ResetForm();
                    CloseModal("OpenModalLargeDeleteVehicle");

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
                }
            });

            await RefreshVehicleList();
        }

        private async Task SubmitAddOtherVehicle()
        {
            try
            {
                managerVehicleViewModel.OtherVehicle.Id = Guid.NewGuid();
                managerVehicleViewModel.OtherVehicle.DateAdded = DateTime.Now;
                managerVehicleViewModel.OtherVehicle.Client = null;
                managerVehicleViewModel.OtherVehicle.IdClient = selectClient;
                await DbContext.OtherVehicles.AddAsync(managerVehicleViewModel.OtherVehicle);
                await DbContext.SaveChangesAsync();

                ResetForm();

                CloseModal("OpenModalLargeAddOtherVehicle");

                await RefreshVehicleList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
            }
        }

        private async Task SubmitEditOtherVehicle()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {
                    DbContext.OtherVehicles.Update(managerVehicleViewModel.OtherVehicle);
                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ResetForm();
                    CloseModal("OpenModalLargeEditOtherVehicle");

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
                }
            });
        }

        private async Task SubmitDeleteOtherVehicle()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {
                    managerVehicleViewModel.OtherVehicle.StatusDataView = StatusData.Delete;
                    DbContext.OtherVehicles.Update(managerVehicleViewModel.OtherVehicle);
                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    ResetForm();
                    CloseModal("OpenModalLargeDeleteOtherVehicle");

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
                }
            });

            await RefreshVehicleList();
        }
        private async Task SubmitDeleteOtherVehicleAllData()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {

                    var vehicleId = managerVehicleViewModel.OtherVehicle.Id;

                    // Supprimer les factures et l'historique des pièces liés aux véhicules
                    await DeleteInvoicesAndHistoryForVehicle(vehicleId, isOtherVehicle: false);
                    await DeleteInvoicesAndHistoryForVehicle(vehicleId, isOtherVehicle: true);

                    DbContext.OtherVehicles.Remove(managerVehicleViewModel.OtherVehicle);

                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Réinitialiser le formulaire et rafraîchir la liste des clients
                    ResetForm();
                    CloseModal("OpenModalLargeDeleteOtherVehicle");

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de la suppréssion des données véhicule");
                }

            });
            await RefreshVehicleList();
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

        private async Task TriggerFileInput()
        {
            await JSRuntime.InvokeVoidAsync("eval",
                "document.getElementById('fileInputOcr').click()");
        }


        private async Task OnFileReadOcr(InputFileChangeEventArgs e)
        {
            var file = e.File;
            // string extractedText = string.Empty;
            if (file != null)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await file.OpenReadStream(maxAllowedSize: 8_500_000).CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var extractedText = await documentAnalyzer.AnalyzeDocument(memoryStream);

                    managerVehicleViewModel.DataOCR = extractedText.DataRead;
                    managerVehicleViewModel.Vehicle = new()
                    {
                        VIN = extractedText.VIN ?? "??",
                        Immatriculation = extractedText.Registration ?? "??",
                        Model = extractedText.Model ?? "??",
                        Mark = extractedText.Mark ?? "??",
                        Type = extractedText.Type ?? "??",
                        //CirculationDate = DateTime.Parse(extractedText.ReleaseDate),
                        //Client = await DbContext.Clients.Where(c => c.Fname == extractedText.Name).FirstOrDefaultAsync() ?? null,
                    };

                    ViewRawOCRData = true;

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de récupération de l'image");
                }
            }
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
