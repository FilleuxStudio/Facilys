namespace Facilys.Components.Services
{
    public class APIWebSiteService
    {
        private readonly HttpClient _httpClient;

        public APIWebSiteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public static bool PostConnectionUser(string email, string password)
        {
            return true;
        }

    }
}
