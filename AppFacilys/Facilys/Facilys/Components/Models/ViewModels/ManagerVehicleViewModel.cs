namespace Facilys.Components.Models.ViewModels
{
    public class ManagerVehicleViewModel
    {
        public Clients Client { get; set; }
        public Vehicles Vehicle { get; set; }
        public OtherVehicles OtherVehicle { get; set; }
        public HistoryPart HistoryPart { get; set; }
        public Invoices Invoices { get; set; }


        public List<Clients> Clients { get; set; }
        public List<Vehicles> Vehicles { get; set; }
        public List<OtherVehicles> OtherVehicles { get; set; }
        public List<HistoryPart> HistoryParts { get; set; }
        public List<Invoices> ListInvoices { get; set; }

        public string DataOCR { get; set; }
    }

    public class ManagerVehicleViewList
    {
        public Clients Client { get; set; }
        public Vehicles Vehicle { get; set; }
        public List<HistoryPart> HistoryParts { get; set; }
        public List<Invoices> Invoices { get; set; }
    }

    public class ManagerOtherVehicleViewList
    {
        public Clients Client { get; set; }
        public OtherVehicles OtherVehicle { get; set; }
        public List<HistoryPart> HistoryParts { get; set; }
        public List<Invoices> Invoices { get; set; }
    }

}
