using Microsoft.AspNetCore.Mvc.Rendering;

namespace Facilys.Components.Models.ViewModels
{
    public class ManagerInvoiceViewModel
    {
        public Invoices Invoice { get; set; }
        public EditionSetting Edition { get; set; }
        public ReferencesIgnored ReferencesIgnored { get; set; }
        public InterestingReferences InterestingReference { get; set; }
        public CompanySettings CompanySettings { get; set; }
        public List<Invoices> Invoices { get; set; }
        public List<ReferencesIgnored> ReferencesIgnoreds { get; set; }
        public List<InterestingReferences> InterestingReferences { get; set; }
        public List<SelectListItem> ClientItems { get; set; }
    }
}
