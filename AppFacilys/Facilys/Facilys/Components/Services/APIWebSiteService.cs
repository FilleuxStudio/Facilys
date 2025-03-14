using Facilys.Components.Constants;
using Facilys.Components.Models;
using PdfSharp.Snippets;
using System.Text.Json;

namespace Facilys.Components.Services
{
    public class APIWebSiteService
    {
        private readonly HttpClient _httpClient;

        public APIWebSiteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:8056");
        }

        public async Task<string> GetKeyAccessApp()
        {
            var response = await _httpClient.GetAsync("get-csrf-token-2025");
            if (response.IsSuccessStatusCode)
            {
                using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return document.RootElement.GetProperty("csrfToken").GetString();
            }
            return "null";
        }

        public async Task<(bool Success, bool IsTeam, object UserData)> PostConnectionUserAsync(string email, string password)
        {
            // Création du FormData
            var formContent = new MultipartFormDataContent
    {
        { new StringContent(email), "email" },
        { new StringContent(password), "password" },
        {new StringContent(EnvironmentApp.AccessToken), "_csrf" }
    };

            // Ajout du header CSRF
            _httpClient.DefaultRequestHeaders.Add("x-csrf-token", EnvironmentApp.AccessToken);
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://facilys.flixmail.fr");

            var response = await _httpClient.PostAsync("api/login", formContent);

            if (!response.IsSuccessStatusCode)
                return (false, false, null);

            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();

            if (content?.Success != true)
                return (false, false, null);

            // Détermination du type d'utilisateur
            var isTeam = content.Message.Contains("Équipe");

            try
            {
                //object userData = isTeam
                //    ? content.Data.Deserialize<TeamData>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                //    : content.Data.Deserialize<UserData>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (true, isTeam, null);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erreur de désérialisation : {ex.Message}");
                return (false, false, null);
            }
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

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public JsonElement Data { get; set; } // Utilisation de JsonElement pour la flexibilité
    }
}
