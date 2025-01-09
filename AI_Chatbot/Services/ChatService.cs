using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AI_Chatbot.Services
{
    public class ChatService : IChatService
    {
        private readonly HttpClient httpClient;
        private const string OllamaBaseUrl = "http://localhost:11434/api/chat";

        public ChatService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> Chatting(ChatRequestDto chatRequest)
        {
            var prompt = $"""
                You are an intelligent assistant designed to classify and handle user queries. Your task is to:

                1. Classify the query as:
                - User Query : Requires login (e.g., setting appointments, viewing payments, prescriptions, insurance details, appointment details etc.).
                - Login Query : Related to authentication operations (login/register).
                - General Query : All other queries that do not require login.

                2. For "User-Specific Query," identify the category from the following list:
                - Get Appointment : If the query inquires about the appointment
                - Set Appointment : If the query involves setting appointment
                - Due Payment : If the query relates to pending payments or billing inquiries.
                - Insurance Details : If the query pertains to insurance-related information (e.g., policy details, status, dates).
                - Prescriptions : If the query is about prescribed medications or prescription-related details.

                3. Response Format:
                - User-Specific:
                ```
                Classification: User Query
                Category : Identified Category
                ```
                - Login/Registration:
                ```
                Classification: Login Query
                ```
                - General:
                ```
                Classification: General Query
                Answer: [Provide answer].
                ```
            Query: {chatRequest.Message}
            """;

            var requestBody = new
            {
                model = "llama3.1:latest",
                messages = new[] { new { role = "user", content = prompt } }
            };

            var response = await httpClient.PostAsJsonAsync(OllamaBaseUrl, requestBody);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
            }

            var rawResponse = await response.Content.ReadAsStringAsync();

            var responseParts = rawResponse.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var contentBuilder = new System.Text.StringBuilder();

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
            return contentBuilder.ToString(); ;
        }

    }
}