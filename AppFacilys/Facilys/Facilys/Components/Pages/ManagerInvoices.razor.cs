using ElectronNET.API;
using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Facilys.Components.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class ManagerInvoices
    {
        readonly ManagerInvoiceViewModel managerInvoiceViewModel = new();
        readonly ModalManagerId modalManager = new();
        ApplicationDbContext DbContext;
        readonly Guid selectInvoice = Guid.Empty;
        private string searchInvoice = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion des factures";
            });

            managerInvoiceViewModel.Invoice = new();
            managerInvoiceViewModel.Invoices = [];
            managerInvoiceViewModel.Edition = new();
            managerInvoiceViewModel.CompanySettings= new();
            modalManager.RegisterModal("OpenModaDeleteInvoice");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UserConnection.LoadCredentialsAsync();
                await LoadDataHeader();
                //await JSRuntime.InvokeVoidAsync("loadScript", "/assets/libs/simple-datatables/umd/simple-datatables.js");
                StateHasChanged(); // Demande un nouveau rendu du composant
            }
        }

        private async Task LoadDataHeader()
        {
            DbContext = await DbContextFactory.CreateDbContextAsync();
            managerInvoiceViewModel.Edition = await DbContext.EditionSettings.FirstOrDefaultAsync() ?? new();
            managerInvoiceViewModel.CompanySettings = await DbContext.CompanySettings.AsNoTracking().FirstOrDefaultAsync();
            managerInvoiceViewModel.Invoices = await DbContext.Invoices.Include(v => v.Vehicle).Include(ov => ov.OtherVehicle).Take(10).OrderByDescending(d => d.DateAdded).ToListAsync();
            managerInvoiceViewModel.InvoiceData = new();
        }

        private async void OpenModalData(string idModal, Guid idInvoice)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            managerInvoiceViewModel.Invoice = await DbContext.Invoices.FindAsync(idInvoice);
            modalManager.OpenModal(idModal);

            StateHasChanged();
        }

        private async void CloseModal(string idModal)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", false);
            modalManager.CloseModal(idModal);
            ResetForm();
            StateHasChanged();
        }

        private void ResetForm()
        {
            managerInvoiceViewModel.Invoice = new(); // Réinitialisez avec un nouvel objet Client
            managerInvoiceViewModel.Invoices.Clear();
            managerInvoiceViewModel.Invoices = [];
            ResetViewModel();
        }

        private async Task SearchInvoiceByNumber()
        {
            managerInvoiceViewModel.Invoices = await DbContext.Invoices.Where(i => i.InvoiceNumber.StartsWith(searchInvoice)).ToListAsync();
            StateHasChanged();
        }

        private async Task UpdatePayment(PaymentMethod newPaymentMethod, Guid idInvoice)
        {
            var invoiceToUpdate = await DbContext.Invoices.FindAsync(idInvoice);
            if (invoiceToUpdate != null)
            {
                invoiceToUpdate.Status = StatusInvoice.Validate;
                invoiceToUpdate.Payment = newPaymentMethod;
                DbContext.Invoices.Update(invoiceToUpdate);
                await DbContext.SaveChangesAsync();
                StateHasChanged(); // Rafraîchir l'UI si nécessaire
            }
        }


        private async Task SubmitDeleteInvoice()
        {
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {

                DbContext.Invoices.Remove(managerInvoiceViewModel.Invoice);
                await DbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Réinitialiser le formulaire et rafraîchir la liste des clients
                ResetForm();
                CloseModal("OpenModaDeleteInvoice");
                await RefreshInvoiceList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de la suppréssion des données de facture");
            }

        }

        private async Task GenerateInvoicePdf(Guid IdInvoice)
        {
            try
            {

                // Récupérer les données nécessaires
                managerInvoiceViewModel.Invoice = await DbContext.Invoices.Include(u => u.User).Where(i => i.Id == IdInvoice).FirstOrDefaultAsync();
                managerInvoiceViewModel.OtherVehicle = await DbContext.OtherVehicles.Include(c => c.Client).Where(o => o.Id == managerInvoiceViewModel.Invoice.IdOtherVehicle).FirstOrDefaultAsync();
                managerInvoiceViewModel.Vehicle = await DbContext.Vehicles.Include(c => c.Client).Where(o => o.Id == managerInvoiceViewModel.Invoice.IdVehicle).FirstOrDefaultAsync();
                var user = await DbContext.Users.FindAsync(managerInvoiceViewModel.Invoice.User.Id);
                managerInvoiceViewModel.HistoryParts = await DbContext.HistoryParts.Include(i => i.Invoice).Where(i => i.Invoice.Id == IdInvoice).ToListAsync();
                Guid? clientId = managerInvoiceViewModel.OtherVehicle?.Client?.Id ?? managerInvoiceViewModel.Vehicle?.Client?.Id;
                managerInvoiceViewModel.Client = await DbContext.Clients.FindAsync(clientId);

                // Récupérer les contacts du client
                var phonesClients = await DbContext.Phones
                    .Where(c => c.Client.Id == clientId)
                    .FirstOrDefaultAsync();
                var emailsClients = await DbContext.Emails
                    .Where(m => m.Client.Id == clientId)
                    .FirstOrDefaultAsync();

                int Km = managerInvoiceViewModel.Vehicle != null ? managerInvoiceViewModel.Vehicle.KM : 0;

                // Générer le nom du fichier
                string fileName = $"{managerInvoiceViewModel.Invoice.InvoiceNumber}-{managerInvoiceViewModel.Client.Fname}-{managerInvoiceViewModel.Client.Lname}-{DateTime.Now:dd-MM-yy}.pdf";

                // Générer le PDF selon le type de design
                byte[] pdfBytesInvoice;
                switch (managerInvoiceViewModel.Edition.TypeDesign)
                {
                    case InvoiceTypeDesign.TypeA:
                        PdfInvoiceType1Service pdfInvoiceType1 = new();
                        pdfBytesInvoice = pdfInvoiceType1.GenerateInvoicePdf(managerInvoiceViewModel, managerInvoiceViewModel.Invoice, Km, phonesClients, emailsClients);
                        break;
                    case InvoiceTypeDesign.TypeB:
                        PdfInvoiceType2Service pdfInvoiceType2 = new();
                        pdfBytesInvoice = pdfInvoiceType2.GenerateInvoicePdf(managerInvoiceViewModel, managerInvoiceViewModel.Invoice, Km, phonesClients, emailsClients);
                        break;
                    case InvoiceTypeDesign.TypeC:
                        PdfInvoiceType3Service pdfInvoiceType3 = new();
                        pdfBytesInvoice = pdfInvoiceType3.GenerateInvoicePdf(managerInvoiceViewModel, managerInvoiceViewModel.Invoice, Km, phonesClients, emailsClients);
                        break;
                    default:
                        throw new InvalidOperationException("Type de design de facture non reconnu");
                }

                // Télécharger le PDF
                await JSRuntime.InvokeVoidAsync("downloadFile", "Facture-" + fileName, pdfBytesInvoice);

                // Sauvegarder le PDF si en mode Electron
                if (HybridSupport.IsElectronActive)
                {
                    await SaveDocuments.SaveDocumentsPDF(
                        managerInvoiceViewModel.Edition.PathSaveFile + "Factures",
                        "Facture-" + fileName,
                        pdfBytesInvoice);
                }

                ResetViewModel();
            }
            catch (Exception ex)
            {
                // Logger l'erreur et/ou afficher un message à l'utilisateur
                Logger.LogError(ex, "Erreur lors de la génération du PDF");
            }
        }

        private void ResetViewModel()
        {
            managerInvoiceViewModel.Invoice = new();
            managerInvoiceViewModel.Vehicle = null;
            managerInvoiceViewModel.OtherVehicle = null;
            managerInvoiceViewModel.HistoryParts = null;
            managerInvoiceViewModel.Client = null;
        }

        private async Task RefreshInvoiceList()
        {
            managerInvoiceViewModel.Invoices.Clear();
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
        }
    }
}
