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
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private readonly IChatHistoryService chatHistory;
        private const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1/chat/completions";
        private string historyText = string.Empty;

        public GeneralQueryService(IHttpClientFactory httpClientFactory, IConfiguration configuration, IChatHistoryService chatHistory)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.chatHistory = chatHistory;
        }

        // This method is used to classify the user query and extract relevant entities.
        public async Task<string> GeneralQuery(int sessionId, string query)
        {
            var httpClient = httpClientFactory.CreateClient();
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

            var data = $$"""
                    You are an advanced AI model that classifies user queries and extracts relevant entities. Your task is to analyze the given query and return response in **valid Json** format:

                    {
                      "intent": "<classified_intent>",
                      "entities": {
                        "email": "<extracted_email_or_null>",
                        "otp": "<extracted_otp_or_null>",
                        "date": "<extracted_date_or_null>",
                        "time": "<extracted_time_or_null>"
                      },
                      "response": "<response_if_general_query_else_null>"
                    }

                    ### Instructions:

                    1. **Classify the query into one of these intents:**
                       - `"Login Query"` → If the query is about login or email verification.
                         - Extract the email if present; otherwise, set `"email": null`.
                       - `"OTP Query"` → If the query contains a 6-digit OTP.
                         - Extract the OTP if present; otherwise, set `"otp": null`.
                       - `"View Appointment Query"` → If the query is about checking a medical appointment.
                       - `"View Prescription Query"` → If the query is about checking medical prescriptions.
                       - `"View Payments Query"` → If the query is about viewing payments or due payments.
                       - `"View Insurance Query"` → If the query is about insurance details.
                       - `"Book Appointment Query"` → If the query is about booking an appointment.
                         - Extract the date and time if present, convert them to:
                           - `"date": "DD/MM/YY"` (Example: `05/03/25`)
                           - `"time": "HH:MM AM/PM"` (Example: `02:30 PM`)
                         - If date or time is missing, set `"date": null` and `"time": null`.
                         - If either date or time is missing, set the missing value as `null`.
                       - `"General Query"` → If the query does not fit the above categories.
                            - Use past conversation history (historyText) to generate a more relavent response.
                            - If the query is related to past messages, respond accordingly.
                            - If there is no relavent past context, generate a general response.

                    2. **Extract Entities (if applicable):**
                       - `"email"` → Extract the email address if the query is a Login Query.
                       - `"otp"` → Extract a 6-digit OTP if present.
                       - `"date"` and `"time"` → If the query contains a date and/or time, extract and format them as:
                         - `"DD/MM/YY"` for date
                         - `"HH:MM AM/PM"` for time
                       - For the values that are not present, set them as `null`.

                    3. **Response Format:**
                       - Ensure that the response is always in **valid JSON** format.
                       - For non-General queries, set `"response": null`.
                       - Do not include extra explanations or text outside the JSON response.
                        
                    ### Examples:

                    #### Example 1: Login Query with Email  
                    **User Query:** *"I forgot my password. My email is john.doe@example.com."*  
                    **Expected JSON Response:**
                    {
                      "intent": "Login Query",
                      "entities": {
                        "email": "john.doe@example.com",
                        "otp": null,
                        "date": null,
                        "time": null
                      },
                      "response": null
                    }
                    
                    ### Example 2 : OTP Query
                    **User Query: ** *"My OTP is 456789. What should I do next?"*
                    **Expected JSON Response:**
                    {
                      "intent": "OTP Query",
                      "entities": {
                        "email": null,
                        "otp": "456789",
                        "date": null,
                        "time": null
                      },
                      "response": null
                    }

                    ### Examplae 3 : Book appointment query
                    **User Query: ** *"I want to book an appointment for 3rd March 2025 at 14:30."*
                    **Expected JSON Response:**
                    {
                      "intent": "Book Appointment Query",
                      "entities": {
                        "email": null,
                        "otp": null,
                        "date": "03/03/25",
                        "time": "02:30 PM"
                      },
                      "response": null
                    }

                    ### Example 4 : General Query with history
                    #### Past Chat History:
                    [
                        {
                            "user" : "I have a fever and cough.",
                            "bot" : "You should take rest and drink plenty of fluids. If symptoms persist, consult a doctor."
                        }
                    ]
                    **User Query: ** *"When will I be fine"*
                    **Expected JSON Response:**
                    {
                      "intent": "General Query",
                      "entities": {
                        "email": null,
                        "otp": null,
                        "date": null,
                        "time": null
                      },
                      "response": "Your fever and cough will improve with proper rest, plenty of fluids, and following your doctor's advice."
                    }

                    ### Example 4 : General Query without history
                    **User Query: ** *"I have a fever and cough."*
                    **Expected JSON Response:**
                    {
                      "intent": "General Query",
                      "entities": {
                        "email": null,
                        "otp": null,
                        "date": null,
                        "time": null
                      },
                      "response": "You should take rest and drink plenty of fluids. If symptoms persist, consult a doctor."
                    }                    
                    """;

            var requestBody = new
            {
                model = "meta-llama/llama-3-8b-instruct:free",
                messages = new[]
                {
                        new { role = "system", content = data },
                        new { role = "assistant", content = historyText},
                        new { role = "user", content = query}
                },
                temperature = 0,
                max_tokens = 1000,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0,
                response_format = "json",
                stream = false
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

