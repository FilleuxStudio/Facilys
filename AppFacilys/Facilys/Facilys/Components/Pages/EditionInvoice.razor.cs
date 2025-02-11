using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using static Facilys.Components.Models.PdfModels.InvoicePDF;

namespace Facilys.Components.Pages
{
    public partial class EditionInvoice
    {
        private ManagerInvoiceViewModel managerInvoiceViewModel = new();
        private string invoiceNumber = string.Empty;
        private string selectedValueClient = string.Empty;
        private string selectedValueVehicle = string.Empty;
        private string searchClient = string.Empty;
        private string km = string.Empty;
        private short actionType;
        private InvoiceData invoiceData = new();

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

            invoiceData.LineRef = Enumerable.Repeat("", managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineDesc = Enumerable.Repeat("", managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineQt = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LinePrice = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineDisc = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineMo = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();
        }

        // Rechercher un client
        private async Task HandleSearchClient(ChangeEventArgs e)
        {
            searchClient = e.Value?.ToString() ?? string.Empty;

            if(searchClient != string.Empty)
            {
                managerInvoiceViewModel.ClientItems = managerInvoiceViewModel.ClientItems
               .Where(c => c.Text.Contains(searchClient, StringComparison.OrdinalIgnoreCase))
               .ToList();
            }
            else
            {
                managerInvoiceViewModel.ClientItems.Clear();
                foreach (var client in await DbContext.Clients.ToListAsync())
                {
                    managerInvoiceViewModel.ClientItems.Add(new SelectListItem(client.Lname + " " + client.Fname, client.Id.ToString()));
                }
            }

            await InvokeAsync(StateHasChanged);
        }

        // selectionner un client
        private async Task HandleSelectionChangedClient(ChangeEventArgs e)
        {
            selectedValueClient = e.Value?.ToString();
            if (Guid.TryParse(selectedValueClient, out Guid clientId))
            {
                managerInvoiceViewModel.VehicleItems.Clear(); // Vider la liste existante
                managerInvoiceViewModel.Client = null;

                managerInvoiceViewModel.Client = await DbContext.Clients.FindAsync(clientId);

                List<Vehicles> vehicles = DbContext.Vehicles
                    .Include(c => c.Client)
                    .Where(v => v.Client.Id == clientId)
                    .ToList();

                foreach (var vehicle in vehicles)
                {
                    managerInvoiceViewModel.VehicleItems.Add(new SelectListItem(vehicle.Immatriculation + " " + vehicle.Mark + " " + vehicle.Model, vehicle.Id.ToString()));
                }

                var otherVehicles = await DbContext.OtherVehicles
                    .Include(v => v.Client)
                    .Where(v => v.Client.Id == clientId)
                    .ToListAsync();

                foreach (var vehicleOther in otherVehicles)
                {
                    managerInvoiceViewModel.VehicleItems.Add(new SelectListItem(vehicleOther.Mark + " " + vehicleOther.Model, vehicleOther.Id.ToString()));
                }

                // Forcer la mise à jour de l'interface
                StateHasChanged();
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


        private async Task OnLinePriceChanged(object value, int index)
        {
            value = value.ToString();
            if(value != string.Empty)
                invoiceData.LinePrice[index] = float.Parse(value.ToString());

            await UpdateLineAmount(index); // Appelle votre méthode pour mettre à jour le montant
        }

        private async Task OnLineDiscChanged(object value, int index)
        {
            value = value.ToString();
            if (value != string.Empty)
                invoiceData.LineDisc[index] = float.Parse(value.ToString());

            await UpdateLineAmount(index); // Appelle votre méthode pour mettre à jour le montant
        }
        /// <summary>
        /// Méthode pour calcul le montant HT
        /// </summary>
        /// <param name="index">id ligne</param>
        /// <returns>calcul</returns>
        private async Task UpdateLineAmount(int index)
        {
            // Assurez-vous que LineQt[index] existe et n'est pas nul
            if (invoiceData.LineQt.Count > index && invoiceData.LineQt[index] != null)
            {
                float price = invoiceData.LinePrice[index] ?? 0.0f;
                float discount = invoiceData.LineDisc[index] ?? 0.0f;
                float quantity = invoiceData.LineQt[index].Value;

                // Calculez le montant HT
                float amount = price * quantity * (1 - discount / 100);

                // Mettez à jour LineMo
                invoiceData.LineMo[index] = (float?)Math.Round(amount, 2); // Arrondi à 2 décimales
            }
        }

        private async Task CreateInvoiceValidSubmit()
        {
            if (actionType == 1)
            {
                
            }
            else if (actionType == 2)
            {
                var pdfBytes = PdfService.GenerateInvoicePdf(invoice);
                await FileService.SaveAsAsync("facture.pdf", pdfBytes, "application/pdf");
                //ou
                var pdfService = new PdfInvoiceService();
                var pdfBytes = pdfService.GenerateInvoicePdf(invoice);
                File.WriteAllBytes("Facture-L0280-AMADOMARIA-2025-02-05.pdf", pdfBytes);
            }
        }

    }

    public class InvoiceData()
    {
        public List<string> LineRef { get; set; } = new();
        public List<string> LineDesc { get; set; } = new();
        public List<float?> LineQt { get; set; } = new();
        public List<float?> LinePrice { get; set; } = new();
        public List<float?> LineDisc { get; set; } = new();
        public List<float?> LineMo { get; set; } = new();
        public string GeneralConditionInvoice { get; set; } = string.Empty;
        public string GeneralConditionOrder { get; set; } = string.Empty;
        public bool PartReturnedCustomer { get; set; } = false;
        public bool CustomerSuppliedPart { get; set; } = false;
    }
}
