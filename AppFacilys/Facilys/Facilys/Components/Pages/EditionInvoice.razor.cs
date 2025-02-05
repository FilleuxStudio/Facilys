using Facilys.Components.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class EditionInvoice
    {
        ManagerInvoiceViewModel managerInvoiceViewModel = new();
        string invoiceNumber = string.Empty;
        string selectedValueClient = string.Empty;
        string selectedValueVehicle = string.Empty;
        string searchClient = string.Empty;
        string searchVehicle = string.Empty;
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Edition de Facturation";
            });

            managerInvoiceViewModel.Edition = new();
            managerInvoiceViewModel.Invoice = new();
            managerInvoiceViewModel.CompanySettings = new();
            managerInvoiceViewModel.ClientItems = new();
            managerInvoiceViewModel.VehicleItems = new();

            await LoadDataHeader();
        }

        private async Task LoadDataHeader()
        {
            managerInvoiceViewModel.Edition = await DbContext.EditionSettings.FirstOrDefaultAsync();
            managerInvoiceViewModel.Invoice = await DbContext.Invoices.OrderByDescending(d => d.InvoiceNumber).LastOrDefaultAsync();
            managerInvoiceViewModel.CompanySettings = await DbContext.CompanySettings.FirstOrDefaultAsync();
            managerInvoiceViewModel.Clients = await DbContext.Clients.ToListAsync();

            if (managerInvoiceViewModel.Invoice == null)
            {
                if (managerInvoiceViewModel.Edition != null)
                    invoiceNumber = managerInvoiceViewModel.Edition.StartNumberInvoice;
                else
                    invoiceNumber = "######";
            }
            else
            {
                string Number = GetLargerInvoiceNumber(managerInvoiceViewModel.Invoice.InvoiceNumber, managerInvoiceViewModel.Edition.StartNumberInvoice);
                if (Number == managerInvoiceViewModel.Edition.StartNumberInvoice)
                    invoiceNumber = managerInvoiceViewModel.Edition.StartNumberInvoice;
                else
                    invoiceNumber = IncrementInvoiceNumber(Number);
            }

            foreach(var client in managerInvoiceViewModel.Clients)
            {
                managerInvoiceViewModel.ClientItems.Add(new SelectListItem(client.Lname + " " + client.Fname, client.Id.ToString()));
            }
            
        }

        /// <summary>
        /// Comparer les numéros de facture
        /// </summary>
        /// <param name="x">numéros de facture</param>
        /// <param name="y">numéros de facture</param>
        /// <returns>le numéro de facture le plus grand</returns>
        private string GetLargerInvoiceNumber(string x, string y)
        {
            if (string.IsNullOrEmpty(x)) return y;
            if (string.IsNullOrEmpty(y)) return x;
            if (x == y) return x;

            int i = 0, j = 0;
            while (i < x.Length && j < y.Length)
            {
                if (char.IsLetter(x[i]) && char.IsLetter(y[j]))
                {
                    if (x[i] > y[j]) return x;
                    if (y[j] > x[i]) return y;
                }
                else if (char.IsDigit(x[i]) && char.IsDigit(y[j]))
                {
                    int numX = 0, numY = 0;
                    while (i < x.Length && char.IsDigit(x[i]))
                        numX = numX * 10 + (x[i++] - '0');
                    while (j < y.Length && char.IsDigit(y[j]))
                        numY = numY * 10 + (y[j++] - '0');
                    if (numX > numY) return x;
                    if (numY > numX) return y;
                    continue;
                }
                else
                {
                    return x[i] > y[j] ? x : y;
                }
                i++; j++;
            }
            return x.Length > y.Length ? x : y;
        }

        /// <summary>
        /// Incrementation du numéro de facture
        /// </summary>
        /// <param name="invoiceNumber">numéro actuelle</param>
        /// <returns>numéro incrémenté</returns>
        private string IncrementInvoiceNumber(string invoiceNumber)
        {
            if (string.IsNullOrEmpty(invoiceNumber))
                return "0001";

            char[] chars = invoiceNumber.ToCharArray();
            int i = chars.Length - 1;

            while (i >= 0)
            {
                if (char.IsDigit(chars[i]))
                {
                    if (chars[i] < '9')
                    {
                        chars[i]++;
                        return new string(chars);
                    }
                    chars[i] = '0';
                    i--;
                }
                else if (char.IsLetter(chars[i]))
                {
                    if (chars[i] < 'Z')
                    {
                        chars[i]++;
                        return new string(chars);
                    }
                    chars[i] = 'A';
                    i--;
                }
            }

            return "A" + new string(chars);
        }

    }
}
