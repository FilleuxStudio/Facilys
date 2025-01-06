using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Facilys.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class Profile
    {
        [Parameter]
        public string UserId { get; set; }

        Users User = new();
        ModalManagerId modalManager = new();
        
        protected override async Task OnInitializedAsync()
        {
            await LoadDataHeader();

            modalManager.RegisterModal("OpenModalEditPicture");
        }

        private async Task LoadDataHeader()
        {
            User = await DbContext.Users.FindAsync(Guid.Parse(UserId));
        }

        private void OnFileSelected()
        {

        }
    }
}
