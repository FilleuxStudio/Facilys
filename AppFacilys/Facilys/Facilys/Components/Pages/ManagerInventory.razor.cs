﻿using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
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
        ApplicationDbContext DbContext;
        private IBrowserFile selectedFile;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion inventaires";
            });

            modalManager.RegisterModal("OpenModalLargeAddInventory");
            modalManager.RegisterModal("OpenModalLargeEditInventory");
            modalManager.RegisterModal("OpenModalDeleteInventory");
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
            InventorysLists = await DbContext.Inventorys.AsNoTracking().ToListAsync();
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
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
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
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Logger.LogError(ex, "Erreur lors de la mise à jour de l'inventaire");
                    throw;
                }
            });

            // Actions post-transaction
            ResetForm();
            CloseModal("OpenModalLargeEditInventory");
        }

        private async Task SubmitDeleteInventory()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {
                    DbContext.Inventorys.Remove(inventory);
                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Logger.LogError(ex, "Erreur lors de la suppression de l'inventaire");
                    throw;
                }
            });

            // Actions post-transaction
            ResetForm();
            CloseModal("OpenModalDeleteInventory");
            await RefreshInventoryList();
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

        private static async Task<string> ConvertToBase64(IBrowserFile file)
        {
            using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] fileBytes = memoryStream.ToArray();
            string base64String = Convert.ToBase64String(fileBytes);
            return $"data:{file.ContentType};base64,{base64String}";
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
