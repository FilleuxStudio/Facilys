namespace Facilys.Components.Services
{
    public class UserConnectionService
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        public string ConnectionString =>
            $"Server={Server};Database={Database};User={UserId};Password={Password};";
    }
}
