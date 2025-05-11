namespace Facilys.Components.Models.ViewModels
{
    public class ManagerQuotationViewModel
    {
        public Quotes Quote { get; set; }
        public Clients Client { get; set; }
        public Vehicles Vehicle { get; set; }
        public OtherVehicles OtherVehicle { get; set; }
        public List<Quotes> Quotes { get; set; }
        public List<Clients> Clients { get; set; }
        public List<Vehicles> Vehicles { get; set; }
    }
}
