using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class ManagerInventory
    {
        List<Inventorys> InventorysLists = new();
        Inventorys inventory = new();
        ModalManagerId modalManager = new();

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion inventaires";
            });

            await LoadDataHeader();

            modalManager.RegisterModal("OpenModalLargeAddInventory");
            modalManager.RegisterModal("OpenModalLargeEditInventory");
            modalManager.RegisterModal("OpenModalDeleteInventory");
        }

        private async Task LoadDataHeader()
        {
            var inventorys = await DbContext.Inventorys.ToListAsync();
        }
        private async void OpenModal(string id)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            StateHasChanged();
        }

        private async void OpenModalData(string idModal, Guid idInventory)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(idModal);
            inventory = await DbContext.Inventorys.Where(i => i.Id == idInventory).FirstOrDefaultAsync();
            StateHasChanged();
        }

        private async void CloseModal(string idModal)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", false);
            modalManager.CloseModal(idModal);
            ResetForm();
            StateHasChanged();
        }

        private void ResetForm()
        {
            InventorysLists = new();
            inventory = new();
        }

        private async Task RefreshInventoryList()
        {
            // Récupérer la liste mise à jour des clients depuis votre service
            InventorysLists = new();;
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
            // await InvokeAsync(StateHasChanged);
            // StateHasChanged();
        }

        private async Task SubmitAddInventory()
        {

        }

        private async Task SubmitEditInventory()
        {

        }

        private async Task SubmitDeleteInventory()
        {

        }
    }
}
