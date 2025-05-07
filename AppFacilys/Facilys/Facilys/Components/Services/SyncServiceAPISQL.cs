using Facilys.Components.Constants;
using MySqlX.XDevAPI;
using SocketIOClient.Transport;
using System.Text;
using System.Text.Json;

namespace Facilys.Components.Services
{
    public class SyncServiceAPISQL
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SyncServiceAPISQL(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Envoie une liste de DTOs vers l'API distante pour synchronisation.
        /// </summary>
        /// <typeparam name="TDto">Type du DTO</typeparam>
        /// <param name="endpoint">Endpoint relatif de l'API (ex: "api/sync/push")</param>
        /// <param name="changes">Liste d'objets à envoyer</param>
        /// <returns>Vrai si la requête aboutit, sinon faux</returns>
        public async Task<bool> PushChangesAsync<TDto>(string endpoint, IEnumerable<TDto> changes)
        {
            try
            {
                // Création du client HTTP
                var client = _httpClientFactory.CreateClient("SyncApi");

                // Ajout des headers CSRF et Origin
                var token = await GetKeyAccessApp();
                client.DefaultRequestHeaders.Add("x-csrf-token", token);
                client.DefaultRequestHeaders.Add("Origin", "https://facilys.flixmail.fr");

                // Préparation du contenu multipart/form-data
                var formContent = new MultipartFormDataContent();

                // Ajout du token CSRF en champ de formulaire
                formContent.Add(new StringContent(token), "_csrf");

                var userEmail = EnvironmentApp.EmailUserConnect;
                formContent.Add(new StringContent(userEmail), "usermail");


                // Sérialisation des changements en JSON et ajout
                var jsonChanges = JsonSerializer.Serialize(changes);
                formContent.Add(new StringContent(jsonChanges, Encoding.UTF8, "application/json"), "changes");

                // Envoi de la requête
                var response = await client.PostAsync(endpoint, formContent);
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException httpEx)
            {
                // TODO: logging, retry, file d'attente
                Console.Error.WriteLine($"Erreur HTTP lors de PushChangesAsync: {httpEx.Message}");
                return false;
            }
        }

        private async Task<string> GetKeyAccessApp()
        {
            var client = _httpClientFactory.CreateClient("SyncApi");
            var response = await client.GetAsync("get-csrf-token-2025");
            if (response.IsSuccessStatusCode)
            {
                using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return document.RootElement.GetProperty("csrfToken").GetString();
            }
            return "null";
        }

    }
}
