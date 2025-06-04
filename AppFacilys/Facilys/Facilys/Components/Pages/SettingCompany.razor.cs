using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class SettingCompany
    {
        [Parameter]
        public string Email { get; set; }
        CompanySettings CompanySettings = new();
        ApplicationDbContext DbContext;
        readonly ModalManagerId modalManager = new();
        private string? PreviewImageBase64 { get; set; }
        private int? UserCount { get; set; }

        [Inject] private EnvironmentApp EnvApp { get; set; }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UserConnection.LoadCredentialsAsync();
                await LoadDataHeader();
                StateHasChanged(); // Demande un nouveau rendu du composant
            }
        }

        //private async Task LoadDataHeader()
        //{
        //    DbContext = await DbContextFactory.CreateDbContextAsync();

        //    CompanySettings = await DbContext.CompanySettings.FirstOrDefaultAsync();
        //    UserCount = await DbContext.Users.CountAsync();

        //    if (CompanySettings == null || CompanySettings.Siret == "null")
        //    {
        //        var (Success, companySettings) = await APIWebSite.PostGetCompanyUserAsync(Email);
        //        if (Success)
        //        {
        //            CompanySettings = companySettings;
        //            await SubmitUpdateOrAddCompany();
        //        }
        //        else
        //        {
        //            EnvApp.AccessToken = await APIWebSite.GetKeyAccessApp();
        //            (Success, companySettings) = await APIWebSite.PostGetCompanyUserAsync(Email);
        //            if (Success)
        //            {
        //                CompanySettings = companySettings;
        //            }
        //        }
        //    }
        //}

        private async Task LoadDataHeader()
        {
            DbContext = await DbContextFactory.CreateDbContextAsync();

            // Récupérer les données locales
            CompanySettings = await DbContext.CompanySettings.FirstOrDefaultAsync();
            UserCount = await DbContext.Users.CountAsync();

            // Vérifier si les données locales sont valides
            bool shouldFetchFromApi = CompanySettings == null ||
                                     string.IsNullOrEmpty(CompanySettings.Siret) ||
                                     CompanySettings.Siret == "null";

            if (shouldFetchFromApi)
            {
                bool success;
                CompanySettings apiSettings;

                // Premier essai de récupération
                (success, apiSettings) = await APIWebSite.PostGetCompanyUserAsync(Email);

                if (!success)
                {
                    // Rafraîchir le token si échec
                    EnvApp.AccessToken = await APIWebSite.GetKeyAccessApp();
                    (success, apiSettings) = await APIWebSite.PostGetCompanyUserAsync(Email);
                }

                if (success)
                {
                    // Sauvegarder l'ID local existant
                    Guid? localId = CompanySettings?.Id;

                    // Mettre à jour avec les données de l'API
                    CompanySettings = apiSettings;

                    // Conserver l'ID local s'il existait
                    if (localId.HasValue)
                    {
                        CompanySettings.Id = localId.Value;
                    }

                    await SubmitUpdateOrAddCompany();
                }
            }
        }
        private async Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            var file = e.File;

            if (file != null)
            {
                using var stream = new MemoryStream();
                await file.OpenReadStream(maxAllowedSize: 1024 * 1024 * 5).CopyToAsync(stream); // Limite de 5 Mo
                var base64Image = Convert.ToBase64String(stream.ToArray());
                PreviewImageBase64 = $"data:{file.ContentType};base64,{base64Image}";

                CompanySettings.Logo = PreviewImageBase64;
            }
        }

        public async Task SubmitUpdateOrAddCompany()
        {
            try
            {
                // Mise à jour via l'API
                await APIWebSite.PutUpdateCompanyAsync(CompanySettings);

                // Vérifier l'existence par ID
                var existingCompany = await DbContext.CompanySettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == CompanySettings.Id);


                if (existingCompany == null)
                {
                    DbContext.CompanySettings.Add(CompanySettings);
                }
                else
                {
                    DbContext.CompanySettings.Update(CompanySettings);
                }

                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erreur lors de la mise à jour de la base de données");
            }
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
