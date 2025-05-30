namespace Facilys.Components.Services
{
    public class EnvironmentApp
    {
        public string FolderData { get; } = "Facilys";

        public string AccessToken { get; set; } = string.Empty;
        public Guid IdUserConnect { get; set; } = Guid.Empty;
        public string EmailUserConnect { get; set; } = string.Empty;
        public string UserDatabaseOnline { get; set; } = string.Empty;
        public string PassDatabaseOnline { get; set; } = string.Empty;
        public string DatabaseOnline { get; set; } = string.Empty;
        public string HostDatabaseOnline { get; set; } = string.Empty;
    }
}
