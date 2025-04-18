using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
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
            managerInvoiceViewModel.Invoices = await DbContext.Invoices.Include(v => v.Vehicle).Include(ov => ov.OtherVehicle).Take(10).OrderByDescending(d => d.DateAdded).ToListAsync();
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

        private async Task RefreshInvoiceList()
        {
            managerInvoiceViewModel.Invoices.Clear();
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
        }
    }
}
