using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClassLibraryFacilys.Models.ViewModels
{
    public class ManagerInvoiceViewModel
    {
        public Invoices Invoice { get; set; }
        public EditionSetting Edition { get; set; }
        public Clients Client { get; set; }
        public Vehicles Vehicle { get; set; }
        public OtherVehicles OtherVehicle { get; set; }
        public ReferencesIgnored ReferencesIgnored { get; set; }
        public InterestingReferences InterestingReference { get; set; }
        public CompanySettings CompanySettings { get; set; }
        public InvoiceData InvoiceData { get; set; }
        public List<Invoices> Invoices { get; set; }
        public List<ReferencesIgnored> ReferencesIgnoreds { get; set; }
        public List<InterestingReferences> InterestingReferences { get; set; }
        public List<HistoryPart> HistoryParts { get; set; }
        public List<Clients> Clients { get; set; }
        public List<Vehicles> Vehicles { get; set; }
        public List<SelectListItem> ClientItems { get; set; }
        public List<SelectListItem> VehicleItems { get; set; }
    }
}
