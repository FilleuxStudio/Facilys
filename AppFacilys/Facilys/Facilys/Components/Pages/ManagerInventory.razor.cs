using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class ManagerInventory
    {
        List<Inventorys> InventorysLists = [];
        Inventorys inventory = new();
        readonly ModalManagerId modalManager = new();
        private IBrowserFile selectedFile;

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
            InventorysLists = await DbContext.Inventorys.ToListAsync();
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
            inventory = new();
        }

        private async Task SubmitAddInventory()
        {
            try
            {
                inventory.Id = Guid.NewGuid();
                inventory.DateAdded = DateTime.Now;

                if (selectedFile != null)
                {
                    inventory.Picture = await ConvertToBase64(selectedFile);
                }
                
                await DbContext.Inventorys.AddAsync(inventory);
                await DbContext.SaveChangesAsync();

                ResetForm();

                CloseModal("OpenModalLargeAddInventory");

                await RefreshInventoryList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
            }
        }

        private void OnFileChangeUpload(InputFileChangeEventArgs e)
        {
            selectedFile = e.File;
        }

        private async Task SubmitEditInventory()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                if (selectedFile != null)
                {
                    inventory.Picture = await ConvertToBase64(selectedFile);
                }

                DbContext.Inventorys.Update(inventory);
                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                ResetForm();
                CloseModal("OpenModalLargeEditInventory");
                await RefreshInventoryList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
            }
        }

        private async Task SubmitDeleteInventory()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {

                DbContext.Inventorys.Remove(inventory);
              
                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Réinitialiser
                ResetForm();
                CloseModal("OpenModalDeleteInventory");
                await RefreshInventoryList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la suppréssion des données inventaire");
            }
        }
        private async Task RefreshInventoryList()
        {
            // Récupérer la liste mise à jour des clients depuis votre service
            InventorysLists.Clear();
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
            // await InvokeAsync(StateHasChanged);
            //StateHasChanged();
        }

        private async Task<string> ConvertToBase64(IBrowserFile file)
        {
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();
            string base64String = Convert.ToBase64String(fileBytes);
            return $"data:{file.ContentType};base64,{base64String}";
        }
    }
}
