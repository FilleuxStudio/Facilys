using ElectronNET.API;
using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Facilys.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Globalization;

namespace Facilys.Components.Pages
{
    public partial class EditionQuotation
    {
        private string QuotationNumber { get; set; } = string.Empty;
        private Guid IdUser = Guid.Empty;
        private string SelectedValueClient { get; set; } = string.Empty;
        private string SelectedValueVehicle { get; set; } = string.Empty;
        private string SearchClient { get; set; } = string.Empty;
        private string currentPhone = string.Empty, currentEmail = string.Empty;
        private int PreloadedLine = 15;
        private int Km { get; set; } = 0;
        private short actionType;
        private readonly ManagerQuotationViewModel managerQuotationViewModel = new();
        private readonly InvoiceData quotationData = new();
        readonly ModalManagerId modalManager = new();
        ApplicationDbContext DbContext;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Edition de devis";
            });

            Init();
            managerQuotationViewModel.CompanySettings = new();

            quotationData.QuotationLines = Enumerable.Range(0, PreloadedLine)
            .Select(_ => new QuotationLine { })
            .ToList();

            modalManager.RegisterModal("OpenModalLargeAddClient");
            modalManager.RegisterModal("OpenModalLargeAddVehicle");

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

        private void Init()
        {
            managerQuotationViewModel.Client = new();
            managerQuotationViewModel.Vehicle = new();
            managerQuotationViewModel.ClientItems = [];
            managerQuotationViewModel.VehicleItems = [];
            managerQuotationViewModel.Quote = new();
            managerQuotationViewModel.Vehicles = [];
            managerQuotationViewModel.PhonesClients = [];
            managerQuotationViewModel.EmailsClients = [];
        }

        private async Task LoadDataHeader()
        {
            DbContext = await DbContextFactory.CreateDbContextAsync();
            managerQuotationViewModel.CompanySettings = await DbContext.CompanySettings.AsNoTracking().Take(1).FirstOrDefaultAsync() ?? new();
            managerQuotationViewModel.Clients = await DbContext.Clients.ToListAsync();
            managerQuotationViewModel.Quote = await DbContext.Quotes.AsNoTracking().Take(1).FirstOrDefaultAsync() ?? new();
            managerQuotationViewModel.Edition = await DbContext.EditionSettings.FirstOrDefaultAsync() ?? new();

            foreach (var client in managerQuotationViewModel.Clients)
            {
                managerQuotationViewModel.ClientItems.Add(new SelectListItem(client.Lname + " " + client.Fname, client.Id.ToString()));
            }


            string Number = GetLargerInvoiceNumber(managerQuotationViewModel.Quote.QuoteNumber, "0001");
            if (Number == managerQuotationViewModel.Quote.QuoteNumber)
                QuotationNumber = IncrementInvoiceNumber(Number);
            else
                QuotationNumber = "0001";

            var user = await AuthService.GetAuthenticatedAsync();
            IdUser = user.Id;
        }

        /// <summary>
        /// Ouvre un modal sans données supplémentaires.
        /// </summary>
        /// <param name="id">L'identifiant du modal à ouvrir.</param>
        private async void OpenModal(string id)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(id);
            Init();
            StateHasChanged();
        }

        /// <summary>
        /// Fermeture du modal
        /// </summary>
        /// <param name="idModal"></param>
        private async void CloseModal(string idModal)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", false);
            modalManager.CloseModal(idModal);
            ResetForm();
            StateHasChanged();
        }

        private void HandleKeyUpPhone(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" || e.Key == ";")
            {
                AddPhone();
            }
        }

        private void HandleKeyDownPhone(KeyboardEventArgs e)
        {
            if (e.Key == "Delete")
            {
                RemoveSelectedPhones();
            }
        }

        private void HandleKeyUpEmail(KeyboardEventArgs e)
        {

            if (e.Key == "Enter" || e.Key == ";")
            {
                AddEmail();
            }
        }

        private void HandleKeyDownEmail(KeyboardEventArgs e)
        {
            if (e.Key == "Delete")
            {
                RemoveSelectedEmails();
            }
        }

        private void AddPhone()
        {
            if (!string.IsNullOrWhiteSpace(currentPhone))
            {
                managerQuotationViewModel.PhonesClients.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Phone = currentPhone.Trim(),
                });
                currentPhone = "";
            }
        }

        private void AddEmail()
        {
            if (!string.IsNullOrWhiteSpace(currentEmail))
            {
                managerQuotationViewModel.EmailsClients.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Email = currentEmail.Trim(),
                });
                currentEmail = "";
            }
        }


        private void HandleEmailSelection(ChangeEventArgs e)
        {
            string[] selectedEmailAddress = (string[])e.Value;
            managerQuotationViewModel.EmailsClient = managerQuotationViewModel.EmailsClients
                .FirstOrDefault(ec => ec.Email == selectedEmailAddress[0]);
        }

        private void HandlePhoneSelection(ChangeEventArgs e)
        {
            string[] selectedPhone = (string[])e.Value;
            managerQuotationViewModel.PhonesClient = managerQuotationViewModel.PhonesClients
                .FirstOrDefault(ec => ec.Phone == selectedPhone[0]);
        }

        private void RemoveSelectedEmails()
        {
            if (managerQuotationViewModel.EmailsClient != null)
            {
                managerQuotationViewModel.EmailsClients.Remove(managerQuotationViewModel.EmailsClient);
                managerQuotationViewModel.EmailsClient = null;
            }

            StateHasChanged();
        }

        private void RemoveSelectedPhones()
        {
            if (managerQuotationViewModel.PhonesClient != null)
            {
                managerQuotationViewModel.PhonesClients.Remove(managerQuotationViewModel.PhonesClient);
                managerQuotationViewModel.PhonesClient = null;
            }
            StateHasChanged();
        }

        // Rechercher numéro facture
        private void HandleQuotationNumber(ChangeEventArgs e)
        {
            QuotationNumber = e.Value?.ToString() ?? string.Empty;
        }

        // Rechercher un client
        private async Task HandleSearchClient(ChangeEventArgs e)
        {
            SearchClient = e.Value?.ToString() ?? string.Empty;

            if (SearchClient != string.Empty)
            {
                managerQuotationViewModel.ClientItems = [.. managerQuotationViewModel.ClientItems.Where(c => c.Text.Contains(SearchClient, StringComparison.OrdinalIgnoreCase))];
            }
            else
            {
                managerQuotationViewModel.ClientItems.Clear();
                foreach (var client in await DbContext.Clients.ToListAsync())
                {
                    managerQuotationViewModel.ClientItems.Add(new SelectListItem(client.Lname + " " + client.Fname, client.Id.ToString()));
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
                managerQuotationViewModel.VehicleItems.Clear(); // Vider la liste existante
                managerQuotationViewModel.Client = null;

                managerQuotationViewModel.Client = await DbContext.Clients.FindAsync(clientId);

                List<Vehicles> vehicles = [.. DbContext.Vehicles
                    .Include(c => c.Client)
                    .Where(v => v.Client.Id == clientId)];

                foreach (var vehicle in vehicles)
                {
                    managerQuotationViewModel.VehicleItems.Add(new SelectListItem(vehicle.Immatriculation + " " + vehicle.Mark + " " + vehicle.Model, vehicle.Id.ToString()));
                }

                var otherVehicles = await DbContext.OtherVehicles
                    .Include(v => v.Client)
                    .Where(v => v.Client.Id == clientId)
                    .ToListAsync();

                foreach (var vehicleOther in otherVehicles)
                {
                    managerQuotationViewModel.VehicleItems.Add(new SelectListItem(vehicleOther.Mark + " " + vehicleOther.Model, vehicleOther.Id.ToString()));
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


        private void OnLinePriceChanged(object value, QuotationLine line)
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

        private void OnLineDiscChanged(object value, QuotationLine line)
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
        private void UpdateLineAmount(QuotationLine line)
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
            quotationData.HT = quotationData.QuotationLines.Sum(line => line.LineMo ?? 0.0f);
            quotationData.TVA = ((quotationData.HT * managerQuotationViewModel.Edition.TVA ?? 0.0f) / 100);
            quotationData.TTC = quotationData.HT + quotationData.TVA;
        }


        private async Task CreateQuotationValidSubmit()
        {
            var otherVehicle = await DbContext.OtherVehicles.FindAsync(Guid.Parse(SelectedValueVehicle));
            var vehicle = await DbContext.Vehicles.FindAsync(Guid.Parse(SelectedValueVehicle));
            var client = await DbContext.Clients.FindAsync(Guid.Parse(SelectedValueClient));
            var user = await DbContext.Users.FindAsync(IdUser);

            Quotes quote = new()
            {
                QuoteNumber = QuotationNumber,
                Client = client,
                OtherVehicle = otherVehicle,
                Vehicle = vehicle,
                TotalAmount = quotationData.TTC,
                Observations = managerQuotationViewModel.Quote.Observations,
                Status = StatusQuote.waiting,
                User = user,
            };

            List<QuotesItems> quotesItems = quotationData.QuotationLines
            .Where(line => !string.IsNullOrWhiteSpace(line.LineRef))
            .Select(line => new QuotesItems
            {
                Id = Guid.NewGuid(),
                Quote = quote,
                PartNumber = line.LineRef,
                Description = line.LineDesc,
                Price = line.LinePrice ?? 0.0f,
                Quantity = (int)(line.LineQt ?? 0),
            })
            .ToList();

            managerQuotationViewModel.Quote = quote;
            managerQuotationViewModel.QuotesItems = quotesItems;
            managerQuotationViewModel.Vehicle = vehicle;
            managerQuotationViewModel.OtherVehicle = otherVehicle;
            managerQuotationViewModel.Client = client;
            managerQuotationViewModel.QuotationData = quotationData;

            if (actionType == 1)
            {

            }
            else if (actionType == 2)
            {

                try
                {
                    var executionStrategy = DbContext.Database.CreateExecutionStrategy();

                    await executionStrategy.ExecuteAsync(async () =>
                    {
                        // Utilisation d'une transaction explicite
                        using var transaction = await DbContext.Database.BeginTransactionAsync();

                        try
                        {
                            await DbContext.Quotes.AddAsync(quote);
                            await DbContext.QuotesItems.AddRangeAsync(quotesItems);

                            // SaveChanges avec acceptAllChangesOnSuccess à false pour plus de sécurité
                            await DbContext.SaveChangesAsync(acceptAllChangesOnSuccess: false);

                            await transaction.CommitAsync();

                            // Accepte les changements seulement après un commit réussi
                            DbContext.ChangeTracker.AcceptAllChanges();
                        }
                        catch
                        {
                            // Rollback explicite en cas d'erreur
                            await transaction.RollbackAsync();

                            // Rejette les changements après un échec
                            DbContext.ChangeTracker.Clear();
                            throw;
                        }
                    });

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de la mise à jour de la base de données");
                }

                PhonesClients phonesClients = await DbContext.Phones.Where(c => c.Client.Id == Guid.Parse(SelectedValueClient)).FirstOrDefaultAsync();
                EmailsClients emailsClients = await DbContext.Emails.Where(m => m.Client.Id == Guid.Parse(SelectedValueClient)).FirstOrDefaultAsync();

                string fileName = QuotationNumber + "-" + managerQuotationViewModel.Client.Fname + "-" + managerQuotationViewModel.Client.Lname + "-" + DateTime.Now.Date.ToString("dd-MM-yy") + ".pdf";
                PdfQuotationType1Service pdfQuotationType = new();
                var pdfQuotation = pdfQuotationType.GenerateQuotationPdf(managerQuotationViewModel, quote, Km, phonesClients, emailsClients);
                await JSRuntime.InvokeVoidAsync("downloadFile", "Devis-" + fileName, pdfQuotation);

                if (HybridSupport.IsElectronActive)
                {
                    await SaveDocuments.SaveDocumentsPDF(managerQuotationViewModel.Edition.PathSaveFile + "Devis", "Devis-" + fileName, pdfQuotation);
                }
            }

            ResetForm();
            StateHasChanged();
        }

        private async Task SubmitAddClient()
        {
            if (managerQuotationViewModel.Client.Fname != "" && managerQuotationViewModel.Client.Lname != "" && managerQuotationViewModel.Client.Address != "")
            {
                try
                {
                    managerQuotationViewModel.Client.Id = Guid.NewGuid();
                    managerQuotationViewModel.Client.DateCreated = DateTime.Now;
                    await DbContext.Clients.AddAsync(managerQuotationViewModel.Client);
                    await DbContext.SaveChangesAsync();

                    if (managerQuotationViewModel.PhonesClients.Count != 0)
                    {
                        for (int i = 0; i < managerQuotationViewModel.PhonesClients.Count; i++)
                        {
                            managerQuotationViewModel.PhonesClients[i].Client = managerQuotationViewModel.Client;
                        }

                        await DbContext.Phones.AddRangeAsync(managerQuotationViewModel.PhonesClients);
                    }

                    if (managerQuotationViewModel.EmailsClients.Count != 0)
                    {
                        for (int i = 0; i < managerQuotationViewModel.EmailsClients.Count; i++)
                        {
                            managerQuotationViewModel.EmailsClients[i].Client = managerQuotationViewModel.Client;
                        }
                        await DbContext.Emails.AddRangeAsync(managerQuotationViewModel.EmailsClients);
                    }

                    await DbContext.SaveChangesAsync();

                    if (HybridSupport.IsElectronActive)
                    {
                        // Envoi vers l'API Node.js avec les DTOs
                        if (HybridSupport.IsElectronActive)
                        {
                            var clientDto = managerQuotationViewModel.Client.ToDto();
                            var phonesDto = managerQuotationViewModel.PhonesClients.Select(p => p.ToDto()).ToArray();
                            var emailsDto = managerQuotationViewModel.EmailsClients.Select(e => e.ToDto()).ToArray();

                            await SyncService.PushChangesAsync("api/query/addclient", new[] { clientDto });
                            await SyncService.PushChangesAsync("api/query/addphone", phonesDto);
                            await SyncService.PushChangesAsync("api/query/addemail", emailsDto);
                        }
                    }

                    managerQuotationViewModel.ClientItems.Clear();
                    managerQuotationViewModel.ClientItems.Add(new SelectListItem(managerQuotationViewModel.Client.Lname + " " + managerQuotationViewModel.Client.Fname, managerQuotationViewModel.Client.Id.ToString()));
                    SelectedValueClient = managerQuotationViewModel.Client.Id.ToString();
                    CloseModal("OpenModalLargeAddClient");

                    await RefreshClientList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
                }
            }
        }

        private async Task SubmitAddVehicle()
        {
            try
            {
                managerQuotationViewModel.Vehicle.Id = Guid.NewGuid();
                managerQuotationViewModel.Vehicle.DateAdded = DateTime.Now;
                managerQuotationViewModel.Vehicle.Client = await DbContext.Clients.FindAsync(Guid.Parse(SelectedValueClient));
                await DbContext.Vehicles.AddAsync(managerQuotationViewModel.Vehicle);
                await DbContext.SaveChangesAsync();


                managerQuotationViewModel.VehicleItems.Clear();
                managerQuotationViewModel.VehicleItems.Add(new SelectListItem(managerQuotationViewModel.Vehicle.Immatriculation + " " + managerQuotationViewModel.Vehicle.Mark + " " + managerQuotationViewModel.Vehicle.Model, managerQuotationViewModel.Vehicle.Id.ToString()));
                SelectedValueVehicle = managerQuotationViewModel.Vehicle.Id.ToString();
                CloseModal("OpenModalLargeAddVehicle");

                await RefreshClientList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, "Erreur lors de l'ajout dans la base de données");
            }
        }

        private void ResetForm()
        {
            managerQuotationViewModel.Client = new(); // Réinitialisez avec un nouvel objet Client
            managerQuotationViewModel.PhonesClients.Clear();
            managerQuotationViewModel.PhonesClients = [];
            managerQuotationViewModel.EmailsClients.Clear();
            managerQuotationViewModel.EmailsClients = [];
            currentPhone = string.Empty;
            currentEmail = string.Empty;
        }

        private async Task RefreshClientList()
        {
            // Récupérer la liste mise à jour des clients depuis votre service
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
            // await InvokeAsync(StateHasChanged);
            // StateHasChanged();
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }

    }
}
