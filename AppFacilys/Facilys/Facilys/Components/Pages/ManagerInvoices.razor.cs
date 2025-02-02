using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class ManagerInvoices
    {
        ManagerInvoiceViewModel managerInvoiceViewModel = new();
        ModalManagerId modalManager = new();
        Guid selectInvoice = Guid.Empty;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion des factures";
            });

            await LoadDataHeader();

            managerInvoiceViewModel.Invoices = new();
        }

        private async Task LoadDataHeader()
        {
            managerInvoiceViewModel.Invoices = await DbContext.Invoices.ToListAsync();
            
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
            managerInvoiceViewModel.Invoices = new();
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
