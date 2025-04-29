using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

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
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UserConnection.LoadCredentialsAsync();
            }
        }
    }
}
