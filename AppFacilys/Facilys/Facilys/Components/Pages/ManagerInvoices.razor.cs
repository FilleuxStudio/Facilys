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
        readonly Guid selectInvoice = Guid.Empty;
        private string searchInvoice = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion des factures";
            });

            await LoadDataHeader();

        }

        private async Task LoadDataHeader()
        {
            managerInvoiceViewModel.Invoices = await DbContext.Invoices.Include(v => v.Vehicle).Include(ov => ov.OtherVehicle).Take(10).OrderByDescending(d => d.DateAdded).ToListAsync();
        }

        private async void OpenModal(string id)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            StateHasChanged();
        }

        private async void OpenModalData(string idModal, Guid idUser)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("loadScript", "/assets/libs/simple-datatables/umd/simple-datatables.js");
                //await JSRuntime.InvokeVoidAsync("loadScript", "/assets/js/pages/datatable.init.js");
            }
        }
    }
}
