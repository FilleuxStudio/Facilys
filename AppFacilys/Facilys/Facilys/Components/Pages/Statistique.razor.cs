using Facilys.Components.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class Statistique
    {
        private IJSObjectReference? chartModule;
        private bool isChartInitialized = false;
        private float totaAmountAnnualInvoice = 0.0f, todayRevenue = 0.0f, monthlyRevenue = 0.0f, maxInvoice = 0.0f, minInvoice = 0.0f;
        private int totalInvoiceAvgMonth = 0;
        private List<InvoiceDto> RecentInvoice = [];
        private readonly double[] monthlyInvoiceTotals = new double[12];
        private int[] paymentData = [];
        private string[] paymentLabels = [];
        public List<string> LastFiveYearsRevenue { get; set; } = [];
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Statistique";
            });

            await LoadDataHeader();

            StateHasChanged();
        }

        private async Task LoadDataHeader()
        {
            totaAmountAnnualInvoice = await DbContext.Invoices.Where(i => i.DateAdded.Year == DateTime.Now.Year).SumAsync(i => i.TotalAmount);
            var invoicesByYear = await DbContext.Invoices.Where(i => i.DateAdded.Year == DateTime.Now.Year).GroupBy(i => i.DateAdded.Month).Select(g => new { Month = g.Key, Count = g.Count() }).ToListAsync();
            await LoadDataInvoicesByMonth();

            if (invoicesByYear.Count != 0)
            {
                totalInvoiceAvgMonth = (int)invoicesByYear.Average(g => g.Count);
            }

            todayRevenue = await DbContext.Invoices.Where(i => i.DateAdded.Date == DateTime.Today).SumAsync(i => i.TotalAmount);

            monthlyRevenue = await DbContext.Invoices.Where(i => i.DateAdded.Year == DateTime.Now.Year && i.DateAdded.Month == DateTime.Now.Month).AverageAsync(i => i.TotalAmount);

            maxInvoice = await DbContext.Invoices.OrderByDescending(i => i.TotalAmount).Select(i => i.TotalAmount).FirstOrDefaultAsync();

            minInvoice = await DbContext.Invoices.OrderBy(i => i.TotalAmount).Select(i => i.TotalAmount).FirstOrDefaultAsync();

            RecentInvoice = await DbContext.Invoices.OrderByDescending(i => i.DateAdded).Take(5).Select(i => new InvoiceDto
            {
                InvoiceId = i.Id,
                OrderNumber = i.OrderNumber,
                TotalAmount = i.TotalAmount,
                DateAdded = i.DateAdded,
                ClientFirstName = i.Vehicle != null ? i.Vehicle.Client.Fname : (i.OtherVehicle != null ? i.OtherVehicle.Client.Fname : "N/A"),
                ClientLastName = i.Vehicle != null ? i.Vehicle.Client.Lname : (i.OtherVehicle != null ? i.OtherVehicle.Client.Lname : "N/A")
            }).ToListAsync();

            var paymentMethodTotals = await DbContext.Invoices
       .GroupBy(i => i.Payment)
       .Select(g => new
       {
           PaymentMethod = g.Key,
           TotalAmount = g.Sum(i => i.TotalAmount)
       })
       .ToListAsync();


            var allPaymentMethods = Enum.GetValues(typeof(PaymentMethod))
                               .Cast<PaymentMethod>()
                               .OrderBy(p => p)
                               .ToList();

            paymentData = new int[allPaymentMethods.Count];
            paymentLabels = new string[allPaymentMethods.Count];

            foreach (var method in allPaymentMethods)
            {
                var index = (int)method;
                var total = paymentMethodTotals.FirstOrDefault(p => p.PaymentMethod == method)?.TotalAmount ?? 0;

                paymentData[index] = (int)Math.Round(total);
                paymentLabels[index] = GetPaymentMethodLabel(method); // Méthode de traduction
            }

            var annualRevenues = await DbContext.Invoices
        .GroupBy(i => i.DateAdded.Year)
        .Select(g => new
        {
            Year = g.Key.ToString(),  // Convertir l'année en chaîne
            TotalRevenue = g.Sum(i => i.TotalAmount).ToString("#.##")  // Formater le revenu avec 2 décimales
        })
        .OrderByDescending(g => g.Year)  // Trier par année décroissante
        .Take(5)  // Prendre les 5 dernières années
        .ToListAsync();

            // Remplir lastFiveYearsRevenue avec des chaînes formatées
            LastFiveYearsRevenue = [.. annualRevenues.Select(x => $"{x.Year}:{x.TotalRevenue}")];
        }

        private static string GetPaymentMethodLabel(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.CashPayment => "Espèces",
                PaymentMethod.BankCards => "Carte Bancaire",
                PaymentMethod.BankTransfers => "Virement Bancaire",
                PaymentMethod.PaymentByCheck => "Chèque",
                PaymentMethod.MobilePayment => "Paiement Mobile",
                PaymentMethod.Cryptocurrencies => "Cryptomonnaies",
                PaymentMethod.NotInformed => "Non Informé",
                _ => method.ToString()
            };
        }

        private async Task LoadDataInvoicesByMonth()
        {
            var invoicesByMonth = await DbContext.Invoices.Where(i => i.DateAdded.Year == DateTime.Now.Year).GroupBy(i => i.DateAdded.Month).Select(g => new { Month = g.Key, Total = g.Sum(i => i.TotalAmount) }).ToListAsync();

            foreach (var item in invoicesByMonth)
            {
                monthlyInvoiceTotals[item.Month - 1] = Math.Round(item.Total); // Mois -1 car index du tableau commence à 0
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && !isChartInitialized)
            {
                try
                {
                    // Vérification du chemin
                    string modulePath = "./assets/js/pages/ecommerce-index.init.js";

                    chartModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", modulePath);

                    // Attendre le rendu complet du DOM
                    await Task.Delay(100);

                    await chartModule.InvokeVoidAsync("initMonthlyIncomeChart", monthlyInvoiceTotals);
                    await chartModule.InvokeVoidAsync("initPaymentChart", paymentData, paymentLabels);

                    isChartInitialized = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur d'initialisation: {ex.Message}");
                }
            }
        }

        public async Task UpdateChartData(double[] newData)
        {
            if (chartModule != null && isChartInitialized)
            {
                await chartModule.InvokeVoidAsync("updateMonthlyIncomeData", [newData]);
            }
        }

        private async Task ChangeData()
        {
            await LoadDataInvoicesByMonth();
            await UpdateChartData(monthlyInvoiceTotals);
        }

    }

    public class InvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public float TotalAmount { get; set; }
        public DateTime DateAdded { get; set; }
        public string ClientFirstName { get; set; } = string.Empty;
        public string ClientLastName { get; set; } = string.Empty;
    }
}
