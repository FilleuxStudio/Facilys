using Facilys.Components.Models.Modal;
using Facilys.Components.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class SettingCompany
    {
        CompanySettings CompanySettings = new();
        ModalManagerId modalManager = new();
        protected override async Task OnInitializedAsync()
        {
            await LoadDataHeader();
        }

        private async Task LoadDataHeader()
        {
            CompanySettings = await DbContext.CompanySettings.FirstOrDefaultAsync();
        }
    }
}
