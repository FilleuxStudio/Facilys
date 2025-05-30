using Facilys.Components.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text;
using System.Text.Json;

namespace Facilys.Components.Services
{
    public class APIWebSiteService
    {
        private readonly HttpClient _httpClient;
        private readonly EnvironmentApp _envApp;
        public APIWebSiteService(HttpClient httpClient, EnvironmentApp envApp)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://facilys.flixmail.fr");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://facilys.flixmail.fr");
            _envApp = envApp;
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

        public async Task<(bool Success, bool IsTeam, UserData UserData)> PostConnectionUserAsync(string email, string password)
        {
            // Création du FormData
            var formContent = new MultipartFormDataContent
    {
        { new StringContent(email), "email" },
        { new StringContent(password), "password" },
        {new StringContent(_envApp.AccessToken), "_csrf" }
    };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/login")
            {
                Content = formContent
            };

            request.Headers.Add("x-csrf-token", _envApp.AccessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return (false, false, null);

            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();

            if (content?.Success != true)
                return (false, false, null);

            // Détermination du type d'utilisateur
            var isTeam = content.Message.Contains("Équipe");

            try
            {
                var document = JsonDocument.Parse(content.Data.GetRawText());
                string lname = string.Empty, fname = string.Empty, company = string.Empty;
                RoleUser roleUser = RoleUser.User;
                if (isTeam)
                {
                    fname = document.RootElement.GetProperty("fname").GetString();
                    lname = document.RootElement.GetProperty("lname").GetString();
                    company = document.RootElement.GetProperty("manager").GetString();
                    switch (document.RootElement.GetProperty("type").GetString())
                    {
                        case "user": roleUser = RoleUser.User; break;
                        case "manager": roleUser = RoleUser.Manager; break;
                        case "administrator": roleUser = RoleUser.Administrator; break;
                        case "master": roleUser = RoleUser.SuperUser; break;
                    }
                }
                else
                {
                    fname = document.RootElement.GetProperty("firstName").GetString();
                    lname = document.RootElement.GetProperty("lastName").GetString();
                    company = document.RootElement.GetProperty("companyName").GetString();
                    roleUser = RoleUser.Manager;
                }

                UserData user = new()
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = document.RootElement.GetProperty("email").GetString(),
                    Password = document.RootElement.GetProperty("password").GetString(),
                    Fname = fname,
                    Lname = lname,
                    Role = roleUser,
                    Company = company,
                    MariadbDb = document.RootElement.GetProperty("mariadbDb").GetString(),
                    MariadbPassword = document.RootElement.GetProperty("mariadbPassword").GetString(),
                    MariadbUser = document.RootElement.GetProperty("mariadbUser").GetString(),
                };

                return (true, isTeam, user);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erreur de désérialisation : {ex.Message}");
                return (false, false, null);
            }
        }

        public async Task<(bool Success, CompanySettings companySettings)> PostGetCompanyUserAsync(string email)
        {
            // Création du FormData
            var formContent = new MultipartFormDataContent
            {
                { new StringContent(email), "email" },
                {new StringContent(_envApp.AccessToken), "_csrf" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "api/company")
            {
                Content = formContent
            };

            request.Headers.Add("x-csrf-token", _envApp.AccessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return (false, null);

            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();

            if (content?.Success != true)
                return (false, null);


            try
            {
                var document = JsonDocument.Parse(content.Data.GetRawText());

                CompanySettings company = new()
                {

                    NameCompany = document.RootElement.GetProperty("companyName").GetString(),
                    Logo = document.RootElement.GetProperty("logo").GetString(),
                    TVA = "NULL",
                    Siret = document.RootElement.GetProperty("siret").GetString(),
                    RIB = "NULL",
                    HeadOfficeAddress = document.RootElement.GetProperty("addressclient").GetString(),
                    BillingAddress = document.RootElement.GetProperty("addressclient").GetString(),
                    LegalStatus = "NULL",
                    RMNumber = "NULL",
                    RCS = "NULL",
                    RegisteredCapital = 1000f,
                    CodeNAF = "NULL",
                    ManagerName = document.RootElement.GetProperty("lastName").GetString().ToUpper() + " " + document.RootElement.GetProperty("firstName").ToString(),
                    Phone = document.RootElement.GetProperty("phone").GetString(),
                    Email = document.RootElement.GetProperty("email").GetString(),
                    WebSite = "NULL",
                };

                return (true, company);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erreur de désérialisation : {ex.Message}");
                return (false, null);
            }
        }

        public async Task<(bool Success, CompanySettings companySettings)> PutUpdateCompanyAsync(CompanySettings updatedSettings)
        {
            // Création du payload avec les nouvelles données
            var updateData = new
            {
                _csrf = _envApp.AccessToken,
                email = updatedSettings.Email, // important !
                companyName = updatedSettings.NameCompany,
                logo = updatedSettings.Logo,
                siret = updatedSettings.Siret,
                addressclient = updatedSettings.HeadOfficeAddress,
                firstName = updatedSettings.ManagerName.Split(' ').FirstOrDefault(),
                lastName = updatedSettings.ManagerName.Split(' ').LastOrDefault(),
                phone = updatedSettings.Phone,
            };

            // Sérialisation sécurisée
            using var jsonContent = new StringContent(
                JsonSerializer.Serialize(updateData),
                Encoding.UTF8,
                "application/json"
            );


            // Configuration des headers
            var request = new HttpRequestMessage(HttpMethod.Put, "api/update-company")
            {
                Content = jsonContent
            };

            request.Headers.Add("x-csrf-token", _envApp.AccessToken);

            // Envoi sécurisé avec gestion d'erreurs améliorée
            try
            {
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    return (false, null);

                // Lecture du résultat
                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();

                if (content?.Success != true)
                    return (false, null);

                var document = JsonDocument.Parse(content.Data.GetRawText());

                CompanySettings company = new()
                {

                    NameCompany = document.RootElement.GetProperty("companyName").GetString(),
                    Logo = document.RootElement.GetProperty("logo").GetString(),
                    TVA = "NULL",
                    Siret = document.RootElement.GetProperty("siret").GetString(),
                    RIB = "NULL",
                    HeadOfficeAddress = document.RootElement.GetProperty("addressclient").GetString(),
                    BillingAddress = document.RootElement.GetProperty("addressclient").GetString(),
                    LegalStatus = "NULL",
                    RMNumber = "NULL",
                    RCS = "NULL",
                    RegisteredCapital = 1000f,
                    CodeNAF = "NULL",
                    ManagerName = document.RootElement.GetProperty("lastName").GetString().ToUpper() + " " + document.RootElement.GetProperty("firstName").ToString(),
                    Phone = document.RootElement.GetProperty("phone").GetString(),
                    Email = document.RootElement.GetProperty("email").GetString(),
                    WebSite = "NULL",
                };

                return (true, company);
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException)
            {
                Console.WriteLine($"Erreur de mise à jour : {ex.Message}");
                return (false, null);
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

    }

    public class UserData
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public RoleUser Role { get; set; }
        public string Company { get; set; }
        public string MariadbUser { get; set; }
        public string MariadbPassword { get; set; }
        public string MariadbDb { get; set; }
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public JsonElement Data { get; set; } // Utilisation de JsonElement pour la flexibilité
    }
}
