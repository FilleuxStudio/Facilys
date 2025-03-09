using System.Net.Http.Json;
using System.Text.Json;

namespace Facilys.Components.Services
{
    public class APIWebSiteService
    {
        private readonly HttpClient _httpClient;

        public APIWebSiteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:8056/api/");
        }

        public async Task<bool> PostConnectionUserAsync(string email, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("login", new { email, password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Success ?? false;
            }
            return false;
        }


        public async Task<UserData> GetUserDataAsync(string userId)
        {
            var response = await _httpClient.GetAsync($"user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserData>();
            }
            return null;
        }

        // Ajoutez d'autres méthodes pour interagir avec votre API selon vos besoins
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }

    public class UserData
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        // Ajoutez d'autres propriétés selon la structure de vos données utilisateur
    }
}
