using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class Statistique
    {
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Statistique";
            });

            await LoadDataHeader();

            StateHasChanged();
        }

        private async Task LoadDataHeader()
        {
            
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("import", "/assets/js/pages/ecommerce-index.init.js");
            }
        }
    }
}
