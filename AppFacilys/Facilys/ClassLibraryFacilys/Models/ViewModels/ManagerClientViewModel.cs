namespace ClassLibraryFacilys.Models.ViewModels
{
    public class ManagerClientViewModel
    {
        public Clients Client { get; set; }
        public PhonesClients PhonesClient { get; set; }
        public EmailsClients EmailsClient { get; set; }
        public Vehicles VehicleClient { get; set; }

        public List<Clients> Clients { get; set; }
        public List<PhonesClients> PhonesClients { get; set; }
        public List<EmailsClients> EmailsClients { get; set; }
        public List<Vehicles> Vehicles { get; set; }

    }

    public class ManagerClienViewtList()
    {
        public Clients Client { get; set; }
        public List<PhonesClients> PhonesClients { get; set; }
        public List<EmailsClients> EmailsClients { get; set; }
        public List<Vehicles> Vehicles { get; set; }
    }
}
