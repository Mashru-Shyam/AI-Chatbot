using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AI_Chatbot.Services
{
    public class GeneralQueryService : IGeneralQueryService
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;
        private const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1/chat/completions";

        public GeneralQueryService(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
        }

        public async Task<string> GeneralQuery(ChatRequestDto chatRequest)
        {
            var prompt = $"""
                Please provide a short and precise answer to the following:
                Query: {chatRequest.Message}
                """;

            var requestBody = new
            {
                model = "meta-llama/llama-3.1-8b-instruct:free",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            try
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", configuration["ApiKey:Key"]);
                
                var response = await httpClient.PostAsJsonAsync(OpenRouterBaseUrl, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API call failed with status {response.StatusCode}: {errorContent}");
                }

                var rawResponse = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(rawResponse);

                var contentProperty = responseJson.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return contentProperty ?? "No response content available.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during GeneralQuery: {ex.Message}");
                throw;
            }
        }
    }
}

