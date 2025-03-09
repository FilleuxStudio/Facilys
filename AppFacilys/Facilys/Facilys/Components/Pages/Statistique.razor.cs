using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class Statistique
    {
        protected override async Task OnInitializedAsync()
        {
            await LoadDataHeader();
        }

        private async Task LoadDataHeader()
        {
            
        }
    }
}
