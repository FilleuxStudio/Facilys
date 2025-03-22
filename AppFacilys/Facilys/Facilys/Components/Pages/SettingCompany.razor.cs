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
        CompanySettings CompanySettings;
        readonly ModalManagerId modalManager = new();
        private string? PreviewImageBase64 { get; set; }
        private int? UserCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await LoadDataHeader();
        }

        private async Task LoadDataHeader()
        {
            //CompanySettings = await DbContext.CompanySettings.FirstOrDefaultAsync();
            CompanySettings ??= new();

            UserCount = await DbContext.Users.CountAsync();

            if (CompanySettings.NameCompany == "" && CompanySettings.Siret == "")
            {
                var (Success, companySettings) = await APIWebSite.PostGetCompanyUserAsync(Email);
                if (Success)
                {
                    CompanySettings = companySettings;
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
                var existingCompany = await DbContext.CompanySettings.FirstOrDefaultAsync(c => c.Id == CompanySettings.Id);
                if (existingCompany == null)
                {
                    await DbContext.AddAsync(CompanySettings);
                }
                else
                {
                    DbContext.Update(CompanySettings);
                }
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
            }
        }
    }
}
