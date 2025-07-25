﻿using ElectronNET.API;
using Facilys.Components.Data;
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
        private string SelectedValueClient { get; set; } = string.Empty;
        private string SelectedValueVehicle { get; set; } = string.Empty;
        private string SearchClient { get; set; } = string.Empty;
        private int Km { get; set; } = 0;
        private short actionType;
        private readonly ManagerInvoiceViewModel managerInvoiceViewModel = new();
        private readonly InvoiceData invoiceData = new();
        ApplicationDbContext DbContext;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Edition de Facturation";
            });

            managerInvoiceViewModel.Edition = new();
            managerInvoiceViewModel.Invoice = new();
            managerInvoiceViewModel.CompanySettings = new();
            managerInvoiceViewModel.ClientItems = [];
            managerInvoiceViewModel.VehicleItems = [];

            invoiceData.InvoiceLines = Enumerable.Range(0, managerInvoiceViewModel.Edition.PreloadedLine)
           .Select(_ => new InvoiceLine { })
           .ToList();
        }




        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await UserConnection.LoadCredentialsAsync();
                await LoadDataHeader();
                StateHasChanged(); // Demande un nouveau rendu du composant
            }
        }
        private async Task LoadDataHeader()
        {
            DbContext = await DbContextFactory.CreateDbContextAsync();

            managerInvoiceViewModel.Edition = await DbContext.EditionSettings.FirstOrDefaultAsync() ?? new();
            managerInvoiceViewModel.Invoice = await DbContext.Invoices.AsNoTracking().OrderByDescending(d => d.InvoiceNumber).FirstOrDefaultAsync();
            managerInvoiceViewModel.CompanySettings = await DbContext.CompanySettings.AsNoTracking().FirstOrDefaultAsync();
            managerInvoiceViewModel.Clients = await DbContext.Clients.ToListAsync();

            if (managerInvoiceViewModel.Edition.PreloadedLine > 0)
            {
                invoiceData.InvoiceLines = Enumerable.Range(0, managerInvoiceViewModel.Edition.PreloadedLine)
          .Select(_ => new InvoiceLine { })
          .ToList();
            }

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

            var user = await AuthService.GetAuthenticatedAsync();
            IdUser = user.Id;
        }

        // Rechercher un client
        private async Task HandleSearchClient(ChangeEventArgs e)
        {
            SearchClient = e.Value?.ToString() ?? string.Empty;

            if (SearchClient != string.Empty)
            {
                managerInvoiceViewModel.ClientItems = [.. managerInvoiceViewModel.ClientItems.Where(c => c.Text.Contains(SearchClient, StringComparison.OrdinalIgnoreCase))];
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
            SelectedValueClient = e.Value?.ToString();
            if (Guid.TryParse(SelectedValueClient, out Guid clientId))
            {
                managerInvoiceViewModel.VehicleItems.Clear(); // Vider la liste existante
                managerInvoiceViewModel.Client = null;

                managerInvoiceViewModel.Client = await DbContext.Clients.FindAsync(clientId);

                List<Vehicles> vehicles = [.. DbContext.Vehicles
                    .Include(c => c.Client)
                    .Where(v => v.Client.Id == clientId)];

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
        private static string GetLargerInvoiceNumber(string x, string y)
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
        private static string IncrementInvoiceNumber(string invoiceNumber)
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

        private static string IncrementPrefix(string prefix)
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


        private void OnLinePriceChanged(object value, InvoiceLine line)
        {
            if (value is string stringValue)
            {
                if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    line.LinePrice = result;
                    UpdateLineAmount(line);
                }
            }
        }

        private void OnLineDiscChanged(object value, InvoiceLine line)
        {
            if (value is string stringValue)
            {
                if (float.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
                {
                    line.LineDisc = result;
                    UpdateLineAmount(line);
                }
            }
        }

        /// <summary>
        /// Méthode pour calcul le montant HT
        /// </summary>
        /// <param name="index">id ligne</param>
        /// <returns>calcul</returns>
        private void UpdateLineAmount(InvoiceLine line)
        {
            if (line.LineQt != null)
            {
                float price = line.LinePrice ?? 0.0f;
                float discount = line.LineDisc ?? 0.0f;
                float quantity = line.LineQt.Value;

                float amount = price * quantity * (1 - discount / 100);
                line.LineMo = (float?)Math.Round(amount, 2);
                CalculateTotals();
            }
        }

        private void CalculateTotals()
        {
            invoiceData.HT = invoiceData.InvoiceLines.Sum(line => line.LineMo ?? 0.0f);
            invoiceData.TVA = ((invoiceData.HT * managerInvoiceViewModel.Edition.TVA ?? 0.0f) / 100);
            invoiceData.TTC = invoiceData.HT + invoiceData.TVA;
        }

        private async Task CreateInvoiceValidSubmit()
        {
            var otherVehicle = await DbContext.OtherVehicles.FindAsync(Guid.Parse(SelectedValueVehicle));
            var vehicle = await DbContext.Vehicles.FindAsync(Guid.Parse(SelectedValueVehicle));
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

            // Création de la liste sans requêtes asynchrones dans le Select
            List<HistoryPart> LineDataPart = invoiceData.InvoiceLines
                .Where(line => !string.IsNullOrWhiteSpace(line.LineRef))
                .Select(line => new HistoryPart
                {
                    Id = Guid.NewGuid(),
                    Invoice = invoice,
                    IdInvoice = invoice.Id,
                    Vehicle = vehicle,
                    OtherVehicle = otherVehicle,
                    PartNumber = line.LineRef,
                    Description = line.LineDesc,
                    Discount = line.LineDisc ?? 0.0f,
                    Price = line.LinePrice ?? 0.0f,
                    Quantity = (int)(line.LineQt ?? 0),
                    KMMounted = Km
                })
                .ToList();

            managerInvoiceViewModel.HistoryParts = LineDataPart;
            managerInvoiceViewModel.Vehicle = vehicle;
            managerInvoiceViewModel.OtherVehicle = otherVehicle;

            if (vehicle != null)
                vehicle.KM = Km;


            if (actionType == 1)
            {
                var executionStrategy = DbContext.Database.CreateExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await DbContext.Database.BeginTransactionAsync();

                    try
                    {
                        // Ajout des entités
                        await DbContext.Invoices.AddAsync(invoice);
                        await DbContext.HistoryParts.AddRangeAsync(LineDataPart);

                        if (vehicle != null)
                            DbContext.Vehicles.Update(vehicle);
                        else if (otherVehicle != null) // Ajout d'une vérification supplémentaire
                            DbContext.OtherVehicles.Update(otherVehicle);

                        // Sauvegarde avec gestion explicite des erreurs
                        var saved = await DbContext.SaveChangesAsync();

                        if (saved > 0) // Vérification que des changements ont bien été sauvegardés
                            await transaction.CommitAsync();
                        else
                        {
                            await transaction.RollbackAsync();
                            Logger.LogWarning("Aucun changement n'a été sauvegardé dans la base de données");
                        }

                        Navigation.NavigateTo("/managerInvoices");
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            // Tentative de rollback en cas d'erreur
                            await transaction.RollbackAsync();
                        }
                        catch (Exception rollbackEx)
                        {
                            Logger.LogError(rollbackEx, "Erreur lors du rollback de la transaction");
                        }

                        Logger.LogError(ex, "Erreur lors de la mise à jour de la base de données");
                        throw; // Re-lancer l'exception pour la gestion ultérieure
                    }
                });

            }
            else if (actionType == 2)
            {

                managerInvoiceViewModel.InvoiceData = invoiceData;

                var executionStrategy = DbContext.Database.CreateExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await DbContext.Database.BeginTransactionAsync();

                    try
                    {
                        // Ajout des entités
                        await DbContext.Invoices.AddAsync(invoice);
                        await DbContext.HistoryParts.AddRangeAsync(LineDataPart);

                        if (vehicle != null)
                            DbContext.Vehicles.Update(vehicle);
                        else if (otherVehicle != null) // Ajout d'une vérification supplémentaire
                            DbContext.OtherVehicles.Update(otherVehicle);

                        // Sauvegarde avec gestion explicite des erreurs
                        var saved = await DbContext.SaveChangesAsync();

                        if (saved > 0) // Vérification que des changements ont bien été sauvegardés
                            await transaction.CommitAsync();
                        else
                        {
                            await transaction.RollbackAsync();
                            Logger.LogWarning("Aucun changement n'a été sauvegardé dans la base de données");
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            // Tentative de rollback en cas d'erreur
                            await transaction.RollbackAsync();
                        }
                        catch (Exception rollbackEx)
                        {
                            Logger.LogError(rollbackEx, "Erreur lors du rollback de la transaction");
                        }

                        Logger.LogError(ex, "Erreur lors de la mise à jour de la base de données");
                        throw; // Re-lancer l'exception pour la gestion ultérieure
                    }
                });

                PhonesClients phonesClients = await DbContext.Phones.Where(c => c.Client.Id == Guid.Parse(SelectedValueClient)).FirstOrDefaultAsync();
                EmailsClients emailsClients = await DbContext.Emails.Where(m => m.Client.Id == Guid.Parse(SelectedValueClient)).FirstOrDefaultAsync();

                string fileName = invoiceNumber + "-" + managerInvoiceViewModel.Client.Fname + "-" + managerInvoiceViewModel.Client.Lname + "-" + DateTime.Now.Date.ToString("dd-MM-yy") + ".pdf";
                byte[] pdfBytesInvoice = null, pdfBytesOrder = null;
                PdfRepairOrderService pdfRepairOrder = new();
                switch (managerInvoiceViewModel.Edition.TypeDesign)
                {
                    case InvoiceTypeDesign.TypeA:
                        PdfInvoiceType1Service pdfInvoiceType1 = new();
                        pdfBytesInvoice = pdfInvoiceType1.GenerateInvoicePdf(managerInvoiceViewModel, invoice, Km, phonesClients, emailsClients);
                        pdfBytesOrder = pdfRepairOrder.GenerateRepairOrderPdf(managerInvoiceViewModel, invoice, Km, phonesClients, emailsClients);
                        break;
                    case InvoiceTypeDesign.TypeB:
                        PdfInvoiceType2Service pdfInvoiceType2 = new();
                        pdfBytesInvoice = pdfInvoiceType2.GenerateInvoicePdf(managerInvoiceViewModel, invoice, Km, phonesClients, emailsClients);
                        pdfBytesOrder = pdfRepairOrder.GenerateRepairOrderPdf(managerInvoiceViewModel, invoice, Km, phonesClients, emailsClients);
                        break;
                    case InvoiceTypeDesign.TypeC:
                        PdfInvoiceType3Service pdfInvoiceType3 = new();
                        pdfBytesInvoice = pdfInvoiceType3.GenerateInvoicePdf(managerInvoiceViewModel, invoice, Km, phonesClients, emailsClients);
                        pdfBytesOrder = pdfRepairOrder.GenerateRepairOrderPdf(managerInvoiceViewModel, invoice, Km, phonesClients, emailsClients);
                        break;
                }

                if (HybridSupport.IsElectronActive)
                {
                    await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Factures", "Facture-" + fileName, pdfBytesInvoice);
                    await SaveDocuments.SaveDocumentsPDF(managerInvoiceViewModel.Edition.PathSaveFile + "Ordre", "Ordre-" + fileName, pdfBytesOrder);
                }

                await JSRuntime.InvokeVoidAsync("downloadFile", "Facture-" + fileName, pdfBytesInvoice);
                await JSRuntime.InvokeVoidAsync("downloadFile", "Ordre-" + fileName, pdfBytesOrder);

            }

            StateHasChanged();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
