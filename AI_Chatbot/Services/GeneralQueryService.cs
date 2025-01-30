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

        public async Task<string> GeneralQuery(string query)
        {
            var prompt = $$"""
                Classify the user query into one of the following intents:  
                1. login: For authentication or login-related queries, may include an email.  
                3. appointment: To view appointment details or checking exsisting appointments.  
                4. schedule: To set, book, or schedule an appointment.  
                5. prescriptions: To view prescriptions details.  
                6. payments: To view payment details.  
                7. insurance: To view insurance details, coverage or status.  
                8. general: Queries that do not match any of the above.  

                If the intent is "general," provide a concise response to the query.  
                Output format:  
                {  
                  "intent": "<classified_intent>",  
                  "response": "<answer_to_query_if_general_or_null>"  
                }  

                Example:  
                Input: "How do I log in to my account?"  
                Output: {"intent": "login", "response": null}  

                Input: "What is the weather today?"  
                Output: {"intent": "general", "response": "The weather is sunny with a high of 28°C."}  

                Query: {{query}}
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

