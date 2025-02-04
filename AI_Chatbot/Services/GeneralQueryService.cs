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
        private readonly IChatHistoryService chatHistory;
        private const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1/chat/completions";
        private string historyText;

        public GeneralQueryService(HttpClient httpClient, IConfiguration configuration, IChatHistoryService chatHistory)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
            this.chatHistory = chatHistory;
        }

        public async Task<string> GeneralQuery(int sessionId, string query)
        {
            var history = await chatHistory.GetChatHistory(sessionId);
            if (history == null || !history.Any())
            {
                historyText = "";
            }
            else
            {
                historyText = string.Join("\n\n", history.Select(i =>
                     $"**User**: {i.UserMessage} \n **Bot**: {i.BotMessage}"));
            }

            var prompt = $$"""
                Classify the user query into one of the following intents:  
                1. login: For authentication or login-related queries, may include an email.  
                3. appointment: To view appointment details or checking exsisting appointments.  
                4. schedule: To set, book, or schedule an appointment.  
                5. prescriptions: To view prescriptions details.  
                6. payments: To view payment details.  
                7. insurance: To view insurance details, coverage or status.  
                8. general: Queries that do not match any of the above.  

                 ### Rules for general query:
                - If the **intent** is **general**, analyze the user's query in relation to past conversation history.  
                  - If the new query refers to previous questions respond accordingly.  
                  - If there is no relevant past context, generate a suitable response based on general knowledge.  

                ### Conversation History:
                {{historyText}}

                Output format follow strictly:  
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

