using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class Profile
    {
        [Parameter]
        public string UserId { get; set; }
        public int CountQuotes = 0, CountInvoices = 0;
        public string PasswordA = string.Empty, PasswordB = string.Empty;

        Users User = new();
        readonly ModalManagerId modalManager = new();
        EditContext editContext;

        protected override async Task OnInitializedAsync()
        {
           
            editContext = new EditContext(this);
            modalManager.RegisterModal("OpenModalEditPicture");
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await _userConnection.LoadCredentialsAsync();
                await LoadDataHeader();
                StateHasChanged(); // Demande un nouveau rendu du composant
            }
        }

        private async Task LoadDataHeader()
        {
            using var context = await DbContextFactory.CreateDbContextAsync();
            User = await context.Users.FindAsync(Guid.Parse(UserId));
            CountQuotes = await context.Quotes.Where(q => q.User.Id == Guid.Parse(UserId)).CountAsync();
            CountInvoices = await context.Invoices.Where(q => q.User.Id == Guid.Parse(UserId)).CountAsync();
        }

        private async Task OnFileSelected(InputFileChangeEventArgs e)
        {
            var file = e.File;
            if (file != null)
            {
                using var context = await DbContextFactory.CreateDbContextAsync();
                using var stream = new MemoryStream();
                await file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 5).CopyToAsync(stream); // Limite de 5 Mo
                var base64Image = Convert.ToBase64String(stream.ToArray());
                User.Picture = $"data:{file.ContentType};base64,{base64Image}";

                context.Users.Update(User);
                await context.SaveChangesAsync();
            }
        }

        private async Task UserInformationSubmit()
        {
            using var context = await DbContextFactory.CreateDbContextAsync();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {

                context.Users.Update(User);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
            }
        }

        private async Task UpdatePasswordSubmit()
        {
            if (PasswordA != PasswordB)
            {
                // Afficher un message d'erreur si les mots de passe ne correspondent pas
                Logger.LogError("Les mots de passe ne correspondent pas.");
                return;
            }
            using var context = await DbContextFactory.CreateDbContextAsync();
            User.Password = Users.HashPassword(PasswordA);
            context.Users.Update(User);
            await context.SaveChangesAsync();

            Logger.LogError($"Nouveau mot de passe : {PasswordB}");

            // Réinitialiser les variables après la soumission, si nécessaire
            PasswordA = string.Empty;
            PasswordB = string.Empty;
        }
    }
}
