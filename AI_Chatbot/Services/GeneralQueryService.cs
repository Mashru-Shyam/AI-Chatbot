using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using System;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AI_Chatbot.Services
{
    public class GeneralQueryService : IGeneralQueryService
    {
        private readonly HttpClient httpClient;
        private const string OllamaBaseUrl = "http://localhost:11434/api/chat";


        public GeneralQueryService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<string> GeneralQuery(ChatRequestDto chatRequest)
        {
            if (string.IsNullOrWhiteSpace(chatRequest?.Message))
            {
                throw new ArgumentException("Query message is required.");
            }

            var prompt = $"""
        You are an intelligent assistant that provide answer to provided query:
        ```
    Query: {chatRequest.Message}
    """;

            var requestBody = new
            {
                model = "llama3.1:latest",
                messages = new[] { new { role = "user", content = prompt } }
            };

            try
            {
                var response = await httpClient.PostAsJsonAsync(OllamaBaseUrl, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API call failed with status {response.StatusCode}: {errorContent}");
                }

                // Deserialize response
                var rawResponse = await response.Content.ReadAsStringAsync();
                var responseParts = rawResponse.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                var contentBuilder = new StringBuilder();

                foreach (var part in responseParts)
                {
                    if (part.Trim().StartsWith("{") && part.Trim().EndsWith("}"))
                    {
                        var jsonDocument = JsonDocument.Parse(part);
                        if (jsonDocument.RootElement.TryGetProperty("message", out var messageProperty) &&
                                messageProperty.TryGetProperty("content", out var contentProperty))
                        {
                            contentBuilder.Append(contentProperty.GetString());
                        }
                    }
                }

                return contentBuilder.ToString();
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow
                Console.WriteLine($"Error during GeneralQuery: {ex.Message}");
                throw;
            }
        }
    }
}
