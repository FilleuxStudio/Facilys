using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Facilys.Components.Models.ViewModels
{
    public class ManagerQuotationViewModel
    {
        public Quotes Quote { get; set; }
        public EditionSetting Edition { get; set; }
        public QuotesItems QuotesItem { get; set; }
        public Clients Client { get; set; }
        public CompanySettings CompanySettings { get; set; }
        public Vehicles Vehicle { get; set; }
        public OtherVehicles OtherVehicle { get; set; }
        public PhonesClients PhonesClient { get; set; }
        public EmailsClients EmailsClient { get; set; }
        public InvoiceData QuotationData { get; set; }
        public List<Quotes> Quotes { get; set; }
        public List<QuotesItems> QuotesItems { get; set; }
        public List<Clients> Clients { get; set; }
        public List<Vehicles> Vehicles { get; set; }
        public List<PhonesClients> PhonesClients { get; set; }
        public List<EmailsClients> EmailsClients { get; set; }
        public List<SelectListItem> ClientItems { get; set; }
        public List<SelectListItem> VehicleItems { get; set; }

    }
}
