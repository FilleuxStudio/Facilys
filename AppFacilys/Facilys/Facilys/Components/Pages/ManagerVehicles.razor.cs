﻿using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Facilys.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Runtime;

namespace Facilys.Components.Pages
{
    public partial class ManagerVehicles
    {
        List<ManagerVehicleViewList> managerVehicleViewLists = new();
        ManagerVehicleViewModel managerVehicleViewModel = new();
        ModalManagerId modalManager = new();
        VINInfo VinInfo = new();
        Guid selectClient = Guid.Empty;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion vehicules";
            });

            await LoadDataHeader();

            managerVehicleViewModel.Client = new();
            managerVehicleViewModel.Vehicle = new();
            managerVehicleViewModel.OtherVehicle = new();
            managerVehicleViewModel.Invoices = new();
            managerVehicleViewModel.HistoryPart = new();

            modalManager.RegisterModal("OpenModalLargeAddVehicle");
            modalManager.RegisterModal("OpenModaSmallInfoVin");
            modalManager.RegisterModal("OpenModalLargeEditVehicle");
            modalManager.RegisterModal("OpenModalLargeDeleteVehicle");
        }

        private async Task LoadDataHeader()
        {

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

            var OtherVehicles = await DbContext.OtherVehicles.Include(c => c.Client).ToListAsync();
            foreach (var vehicle in OtherVehicles)
            {
                managerVehicleViewLists.Add(new()
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
                managerVehicleViewModel.Client = await DbContext.Clients.FindAsync(managerVehicleViewModel.Vehicle.Client.Id);
            else
                managerVehicleViewModel.Client = await DbContext.Clients.FindAsync(managerVehicleViewModel.OtherVehicle.Client.Id);

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

        private void ResetForm()
        {
            managerVehicleViewModel.Client = new();
            managerVehicleViewModel.Vehicle = new();
            managerVehicleViewModel.HistoryPart = new();
        }

        private async Task RefreshVehicleList()
        {
            // Récupérer la liste mise à jour des clients depuis votre service
            managerVehicleViewLists.Clear();
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
                managerVehicleViewModel.Vehicle.Client = await DbContext.Clients.FindAsync(selectClient);
                await DbContext.Vehicles.AddAsync(managerVehicleViewModel.Vehicle);
                await DbContext.SaveChangesAsync();

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
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                DbContext.Vehicles.Update(managerVehicleViewModel.Vehicle);
                await DbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                ResetForm();
                CloseModal("OpenModalLargeEditVehicle");
                await RefreshVehicleList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
            }
        }

        private async Task SubmitDeleteVehicleAllData()
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
                await RefreshVehicleList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la suppréssion des données véhicule");
            }
        }

        private async Task SubmitDeleteVehicle()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                managerVehicleViewModel.Vehicle.StatusDataView = StatusData.Delete;
                DbContext.Vehicles.Update(managerVehicleViewModel.Vehicle);
                await DbContext.SaveChangesAsync();


                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                ResetForm();
                CloseModal("OpenModalLargeDeleteVehicle");
                await RefreshVehicleList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
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
            }
            else
            {
                await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM Invoices WHERE IdVehicle = {0}", vehicleId);
            }
        }
    }
}
