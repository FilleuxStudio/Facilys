using Facilys.Components.Models;
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

           var vehicles = await DbContext.Vehicles.Include(c => c.Client).ToListAsync();
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
            StateHasChanged();
        }

        private async void OpenModalData(string id, string vin)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            VINDecoderService decoderService = new VINDecoderService();
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

        private async Task RefreshClientList()
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

                await RefreshClientList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
            }
        }

        private async Task SubmitEditVehicle()
        {
           
        }

        private async Task SubmitDeleteVehicleAllData()
        {
      
        }

        private async Task SubmitDeleteVehicle()
        {
         
        }
    }
}
