﻿using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Pages
{
    public partial class SettingCompany
    {
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
            CompanySettings = await DbContext.CompanySettings.FirstOrDefaultAsync();
            CompanySettings ??= new();

            UserCount = await DbContext.Users.CountAsync();
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
