using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;

namespace Facilys.Components.Pages
{
    public partial class ManagerQuotation
    {
        readonly ManagerQuotationViewModel managerQuotationViewModel = new();
        readonly ModalManagerId modalManager = new();
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion des devis";
            });

            await LoadDataHeader();

            StateHasChanged();
        }

        private async Task LoadDataHeader()
        {

        }
    }
}
