using Facilys.Components.Models;
using Facilys.Components.Models.ViewModels;
using Facilys.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Globalization;

namespace Facilys.Components.Pages
{
    public partial class EditionInvoice
    {
        private string invoiceNumber = string.Empty;
        private Guid IdUser = Guid.Empty;
        private string selectedValueClient { get; set; } = string.Empty;
        private string selectedValueVehicle { get; set; } = string.Empty;
        private string searchClient { get; set; } = string.Empty;
        private int km { get; set; } = 0;
        private short actionType;
        private ManagerInvoiceViewModel managerInvoiceViewModel = new();
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
            managerInvoiceViewModel.Invoice = await DbContext.Invoices.OrderByDescending(d => d.InvoiceNumber).FirstOrDefaultAsync();
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
                if (Number == managerInvoiceViewModel.Invoice.InvoiceNumber)
                    invoiceNumber = IncrementInvoiceNumber(Number);
                else
                    invoiceNumber = managerInvoiceViewModel.Edition.StartNumberInvoice;
            }

            foreach (var client in managerInvoiceViewModel.Clients)
            {
                managerInvoiceViewModel.ClientItems.Add(new SelectListItem(client.Lname + " " + client.Fname, client.Id.ToString()));
            }

            invoiceData.LineRef = Enumerable.Repeat("", managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineDesc = Enumerable.Repeat("", managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineQt = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LinePrice = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineDisc = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();
            invoiceData.LineMo = Enumerable.Repeat<float?>(0.0f, managerInvoiceViewModel.Edition.PreloadedLine).ToList();

            var user = await AuthService.GetAuthenticatedAsync();
            IdUser = user.Id;
        }

        // Rechercher un client
        private async Task HandleSearchClient(ChangeEventArgs e)
        {
            searchClient = e.Value?.ToString() ?? string.Empty;

            if (searchClient != string.Empty)
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

            // Séparation préfixe alphabétique et suffixe numérique
            int splitPos = 0;
            while (splitPos < invoiceNumber.Length && char.IsLetter(invoiceNumber[splitPos]))
                splitPos++;

            string prefix = invoiceNumber[..splitPos];
            string suffix = invoiceNumber[splitPos..];

            bool overflow = false;

            // Gestion du suffixe numérique
            if (suffix.Length == 0)
            {
                suffix = "0000";
            }
            else
            {
                try
                {
                    long number = long.Parse(suffix) + 1;
                    string newSuffix = number.ToString($"D{suffix.Length}");

                    if (newSuffix.Length > suffix.Length)
                    {
                        overflow = true;
                        suffix = new string('0', suffix.Length);
                    }
                    else
                    {
                        suffix = newSuffix;
                    }
                }
                catch
                {
                    suffix = new string('0', suffix.Length);
                    overflow = true;
                }
            }

            // Gestion du préfixe alphabétique en cas de débordement
            if (overflow)
            {
                prefix = IncrementPrefix(prefix);

                if (prefix.Length == 1 && prefix[0] == 'A' && string.IsNullOrEmpty(invoiceNumber[..splitPos]))
                    suffix = "0000";
            }

            return prefix + suffix;
        }

        private string IncrementPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return "A";

            char[] chars = prefix.ToCharArray();

            for (int i = chars.Length - 1; i >= 0; i--)
            {
                if (chars[i] != 'Z')
                {
                    chars[i]++;
                    return new string(chars);
                }
                chars[i] = 'A';
            }

            return "A" + new string(chars);
        }


        private async Task OnLinePriceChanged(object value, int index)
        {
            if (value is string stringValue)
            {
                if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    invoiceData.LinePrice[index] = result;
                    await UpdateLineAmount(index);
                }
            }

        }

        private async Task OnLineDiscChanged(object value, int index)
        {
            if (value is string stringValue)
            {
                if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    invoiceData.LineDisc[index] = result;
                    await UpdateLineAmount(index);
                }
            }
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

                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            float totalHT = 0;
            for (int i = 0; i < managerInvoiceViewModel.Edition.PreloadedLine; i++)
            {
                totalHT += invoiceData.LineMo[i] ?? 0.0f;
            }
            invoiceData.HT = totalHT;
            invoiceData.TVA = ((totalHT * managerInvoiceViewModel.Edition.TVA ?? 0.0f) / 100); // Supposons un taux de TVA de 20%
            invoiceData.TTC = invoiceData.HT + invoiceData.TVA;
        }

        private async Task CreateInvoiceValidSubmit()
        {
            var otherVehicle = await DbContext.OtherVehicles.FindAsync(Guid.Parse(selectedValueVehicle));
            var vehicle = await DbContext.Vehicles.FindAsync(Guid.Parse(selectedValueVehicle));
            var user = await DbContext.Users.FindAsync(IdUser);

            Invoices invoice = new()
            {
                InvoiceNumber = invoiceNumber,
                OrderNumber = invoiceNumber,
                OtherVehicle = otherVehicle,
                Vehicle = vehicle,
                Payment = PaymentMethod.NotInformed,
                Status = StatusInvoice.OnHold,
                TotalAmount = invoiceData.TTC,
                Observations = invoiceData.GeneralConditionInvoice,
                RepairType = invoiceData.GeneralConditionOrder,
                User = user
            };

            List<HistoryPart> LineDataPart = new();

            for (int i = 0; i < invoiceData.LineRef.Count; i++)
            {
                if (invoiceData.LineRef[i] != string.Empty || invoiceData.LineRef[i] != null)
                {
                    LineDataPart.Add(new HistoryPart()
                    {
                        Id = Guid.NewGuid(),
                        Invoice = invoice,
                        Vehicle = await DbContext.Vehicles.FindAsync(Guid.Parse(selectedValueVehicle)),
                        OtherVehicle = await DbContext.OtherVehicles.FindAsync(Guid.Parse(selectedValueVehicle)),
                        PartNumber = invoiceData.LineRef[i],
                        Description = invoiceData.LineDesc[i],
                        Discount = invoiceData.LineDisc[i] ?? 0.0f,
                        Price = invoiceData.LinePrice[i] ?? 0.0f,
                        Quantity = invoiceData.LineQt[i] ?? 0.0f,
                        KMMounted = km
                    });
                }
            }

            managerInvoiceViewModel.HistoryParts = LineDataPart;

            var Vehicle = await DbContext.Vehicles.FindAsync(Guid.Parse(selectedValueVehicle));
            var OtherVehicle = await DbContext.OtherVehicles.FindAsync(Guid.Parse(selectedValueVehicle));

            managerInvoiceViewModel.Vehicle = Vehicle;
            managerInvoiceViewModel.OtherVehicle = OtherVehicle;

            if (Vehicle != null)
                Vehicle.KM = km;


            if (actionType == 1)
            {

            }
            else if (actionType == 2)
            {

                managerInvoiceViewModel.InvoiceData = invoiceData;

                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {

                    await DbContext.Invoices.AddAsync(invoice);
                    await DbContext.HistoryParts.AddRangeAsync(LineDataPart);
                    DbContext.Vehicles.Update(Vehicle);

                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
                }

                PhonesClients phonesClients = await DbContext.Phones.Where(c => c.Client.Id == Guid.Parse(selectedValueClient)).FirstOrDefaultAsync();
                EmailsClients emailsClients = await DbContext.Emails.Where(m => m.Client.Id == Guid.Parse(selectedValueClient)).FirstOrDefaultAsync();

                string fileName = invoiceNumber + "-" + managerInvoiceViewModel.Client.Fname + "-" + managerInvoiceViewModel.Client.Lname + "-" + DateTime.Now.Date.ToString("dd-MM-yy") + ".pdf";
                byte[] pdfBytesInvoice = null, pdfBytesOrder = null;
                PdfRepairOrderService pdfRepairOrder = new();
                switch (managerInvoiceViewModel.Edition.TypeDesign)
                {
                    case InvoiceTypeDesign.TypeA:
                        PdfInvoiceType1Service pdfInvoiceType1 = new();
                        pdfBytesInvoice = pdfInvoiceType1.GenerateInvoicePdf(managerInvoiceViewModel, invoice, km, phonesClients, emailsClients);
                        pdfBytesOrder = pdfRepairOrder.GenerateRepairOrderPdf(managerInvoiceViewModel, invoice, km, phonesClients, emailsClients);
                        await JSRuntime.InvokeVoidAsync("downloadFile", "Facture-" + fileName, pdfBytesInvoice);
                        await JSRuntime.InvokeVoidAsync("downloadFile", "Ordre-" + fileName, pdfBytesOrder);

                        await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Factures", "Facture-" + fileName, pdfBytesInvoice);
                        await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Ordre", "Ordre-" + fileName, pdfBytesOrder);
                        break;
                    case InvoiceTypeDesign.TypeB:
                        PdfInvoiceType2Service pdfInvoiceType2 = new();
                        pdfBytesInvoice = pdfInvoiceType2.GenerateInvoicePdf(managerInvoiceViewModel, invoice, km, phonesClients, emailsClients);
                        pdfBytesOrder = pdfRepairOrder.GenerateRepairOrderPdf(managerInvoiceViewModel, invoice, km, phonesClients, emailsClients);
                        await JSRuntime.InvokeVoidAsync("downloadFile", "Facture-" + fileName, pdfBytesInvoice);
                        await JSRuntime.InvokeVoidAsync("downloadFile", "Ordre-" + fileName, pdfBytesOrder);

                        await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Factures", "Facture-" + fileName, pdfBytesInvoice);
                        await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Ordre", "Ordre-" + fileName, pdfBytesOrder);
                        break;
                    case InvoiceTypeDesign.TypeC:
                        //PdfInvoiceType3Service pdfInvoiceType3 = new();
                        //pdfBytesInvoice = pdfInvoiceType3.GenerateInvoicePdf(managerInvoiceViewModel, invoice, km, phonesClients, emailsClients);
                        //pdfBytesOrder = pdfRepairOrder.GenerateRepairOrderPdf(managerInvoiceViewModel, invoice, km, phonesClients, emailsClients);
                        //await JSRuntime.InvokeVoidAsync("downloadFile", "Facture-" + fileName, pdfBytesInvoice);
                        //await JSRuntime.InvokeVoidAsync("downloadFile", "Ordre-" + fileName, pdfBytesOrder);

                        //await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Factures", "Facture-" + fileName, pdfBytesInvoice);
                        //await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Ordre", "Ordre-" + fileName, pdfBytesOrder);
                        break;
                }

            }

            StateHasChanged();
        }
    }
}
