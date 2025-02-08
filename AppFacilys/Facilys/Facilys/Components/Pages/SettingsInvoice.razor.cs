using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class SettingsInvoice
    {
        readonly ManagerInvoiceViewModel managerInvoiceViewModel = new();

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Paramétrages de Facturation";
            });

            await LoadDataHeader();

            managerInvoiceViewModel.Edition ??= new();
        }

        private async Task LoadDataHeader()
        {
            managerInvoiceViewModel.Edition = await DbContext.EditionSettings.FirstOrDefaultAsync();
        }

        private async Task SubmitSaveSettingInvoice()
        {
            try
            {
                if(managerInvoiceViewModel.Edition.Id == Guid.Empty)
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
    }
}
