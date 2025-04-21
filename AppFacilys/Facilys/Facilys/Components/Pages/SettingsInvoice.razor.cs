using Facilys.Components.Data;
using Facilys.Components.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class SettingsInvoice
    {
        readonly ManagerInvoiceViewModel managerInvoiceViewModel = new();
        ApplicationDbContext DbContext;
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Paramétrages de Facturation";
            });

            managerInvoiceViewModel.Edition = new();
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
            managerInvoiceViewModel.Edition = await DbContext.EditionSettings.FirstOrDefaultAsync() ?? new();
        }

        private async Task SubmitSaveSettingInvoice()
        {
            try
            {
                if (managerInvoiceViewModel.Edition.Id == Guid.Empty)
                {
                    managerInvoiceViewModel.Edition.Id = Guid.NewGuid();
                    await DbContext.EditionSettings.AddAsync(managerInvoiceViewModel.Edition);
                }
                else
                {
                    DbContext.EditionSettings.Update(managerInvoiceViewModel.Edition);
                }

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
            }
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
