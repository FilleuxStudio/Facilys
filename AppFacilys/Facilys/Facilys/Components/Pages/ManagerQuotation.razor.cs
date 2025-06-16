using Facilys.Components.Data;
using Facilys.Components.Models.Modal;
using Facilys.Components.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace Facilys.Components.Pages
{
    public partial class ManagerQuotation
    {
        readonly ManagerQuotationViewModel managerQuotationViewModel = new();
        readonly ModalManagerId modalManager = new();
        ApplicationDbContext DbContext;
        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(() =>
            {
                PageTitleService.CurrentTitle = "Gestion des devis";
            });

            managerQuotationViewModel.Quotes = [];
            managerQuotationViewModel.Quote = new();

            modalManager.RegisterModal("OpenModalDeleteQuation");
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
            managerQuotationViewModel.Quotes = await DbContext.Quotes.Include(c => c.Client).Include(v => v.Vehicle).Take(20).OrderByDescending(d => d.DateAdded).ToListAsync();
        }


        /// <summary>
        /// Ouvre un modal avec des données spécifiques
        /// </summary>
        private async void OpenModalData(string idModal, Guid idQuote)
        {
            await JSRuntime.InvokeVoidAsync("modifyBodyForModal", true);
            modalManager.OpenModal(idModal);
            managerQuotationViewModel.Quote = await DbContext.Quotes.Where(i => i.Id == idQuote).FirstOrDefaultAsync();
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

        private void ResetForm()
        {
            managerQuotationViewModel.Client = new(); // Réinitialisez avec un nouvel objet Client
            managerQuotationViewModel.Vehicle = new();
            managerQuotationViewModel.Quotes = [];
            managerQuotationViewModel.Quote = new();
        }

        private async Task SubmitDeleteQuatation()
        {
            var executionStrategy = DbContext.Database.CreateExecutionStrategy();
            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await DbContext.Database.BeginTransactionAsync();
                try
                {
                    var quoteId = managerQuotationViewModel.Quote.Id;

                    // Suppression avec Entity Framework (plus propre)
                    var quotesItems = await DbContext.QuotesItems
                        .Where(qi => qi.IdQuote == quoteId)
                        .ToListAsync();

                    if (quotesItems.Any())
                    {
                        DbContext.QuotesItems.RemoveRange(quotesItems);
                    }

                    var quote = await DbContext.Quotes.FindAsync(quoteId);
                    if (quote != null)
                    {
                        DbContext.Quotes.Remove(quote);
                    }

                    await DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Logger.LogError(ex, "Erreur lors de la suppression des données devis");
                    throw;
                }
            });

            // Actions post-transaction
            ResetForm();
            CloseModal("OpenModalDeleteQuation");
            await RefreshQuotationList();
        }

        private async Task RefreshQuotationList()
        {
            // Récupérer la liste mise à jour des clients depuis votre service
            managerQuotationViewModel.Quotes.Clear();
            await LoadDataHeader();
            await InvokeAsync(StateHasChanged);
            // await InvokeAsync(StateHasChanged);
            // StateHasChanged();
        }

    }
}
