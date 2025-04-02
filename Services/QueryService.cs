using AI_Chatbot.Interfaces;
using System.Text.Json;

namespace AI_Chatbot.Services
{
    public class QueryService : IQueryService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IChatHistoryService chatHistory;
        private const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1/chat/completions";
        private string historyText = string.Empty;
        private readonly string apiKey = Environment.GetEnvironmentVariable("OPEN_ROUTER_API_KEY") ?? throw new InvalidOperationException("API_KEY environment variable is not set.");

        public QueryService(IHttpClientFactory httpClientFactory, IChatHistoryService chatHistory)
        {
            this.httpClientFactory = httpClientFactory;
            this.chatHistory = chatHistory;
        }
        public async Task<string> EntityExtraction(int sessionId, string query)
        {
            var httpClient = httpClientFactory.CreateClient();
            string currentDate = DateTime.Now.ToString("dd/MM/yy");
            var data = $$"""
                    You are an advanced AI model that extracts entities from user queries. Your task is to analyze the given query and return response in format:
                    {
                      "entities": {
                        "email": "<extracted_email_or_null>",
                        "otp": "<extracted_otp_or_null>",
                        "date": "<extracted_date_or_null>",
                        "time": "<extracted_time_or_null>"
                      }
                    }

                    ### Instructions:
                    1. **Extract the following entities from the query:**
                       - `"date"` → If the query contains a date. Extract date and convert it to a standard format (dd/mm/yy) 
                            - Today’s date is: {{currentDate}}*. Use this to interpret relative dates correctly. Recognize relative dates like "yesterday", "today", "tomorrow", "next Monday", and convert them to "dd/mm/yyformat (Example: "05/03/25"). If the year is not specified, use the current year by default.
                            - Extract the date if present; otherwise, set `"date": null`.
                       - `"time"` → If the query contains a time. Extract time and convert it to a standard format (HH:mm AM/PM)
                            - Example: `10:30 AM`
                            - Extract the time if present; otherwise, set `"time": null`.
                       - `"email"` → If the query contains an email address. Extract the email address.
                            - Extract the email if present; otherwise, set `"email": null`.
                       - `"otp"` → If the query contains an 6 digit otp. Extract the otp.
                            - Extract the otp if present; otherwise, set `"otp": null`.
                       
                    2. **Response Format:**
                       - Ensure that the response is always in **valid JSON** format.
                       - For non-detected entities, do not include them in the response.
                       - Do not include extra explanations or text outside the JSON response.
                        
                    ### Examples:
                    #### Example 1: Date Entity.
                    **User Query:** *"Book an appointment on 25th December."*  
                    **Expected JSON Response:**
                    {
                      "entities": {
                        "date": "25/12/25"
                        "email": null,
                        "otp": null,
                        "time": null
                      }
                    }
                    ### Examplae 2 : Email Entity
                    **User Query: ** *"My email is s@gmail.com
                    **Expected JSON Response:**
                    {
                      "entities": {
                        "email": "s@gmail.com"
                        "date": null,
                        "otp": null,
                        "time": null
                      }
                    }
                    #### Example 3: Time Entity.
                    **User Query:** *"14:00"*  
                    **Expected JSON Response:**
                    {
                      "entities": {
                        "time": "02:00 PM"
                        "email": null,
                        "otp": null,
                        "date": null
                      }
                    }
                    """;

            var requestBody = new
            {
                model = "meta-llama/llama-3.3-70b-instruct:free",
                messages = new[]
                {
                    new { role = "system", content = data },
                    new { role = "user", content = query}
                },
                temperature = 0,
                max_tokens = 1000,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0,
                stream = false
            };

            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await httpClient.PostAsJsonAsync(OpenRouterBaseUrl, requestBody);
            if (response.IsSuccessStatusCode)
            {
                var rawResponse = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(rawResponse);
                var contentProperty = responseJson.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return contentProperty ?? "No response content available.";
            }
            else
            {
                return "I am sorry, I could not understand your query. Please try again.";
            }
        }

        public async Task<string> IntentClassification(int sessionId, string query)
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
                    You are an advanced AI model that classifies user queries. Your task is to analyze the given query and return response in **valid Json** format:
                    {
                      "intent": "<classified_intent>",
                      "response": "<response_if_general_query_else_null>"
                    }

                    ### Instructions:

                    1. **Classify the query into one of these intents:**
                       - `"Login"` → If the query is about login or email verification.
                       - `"Appointment"` → If the query is about checking a medical appointment.
                       - `"Prescription"` → If the query is about checking medical prescriptions.
                       - `"Payment"` → If the query is about payments details or due payments.
                       - `"Insurance"` → If the query is about insurance details.
                       - `"Schedule"` → If the query is about booking or scheduling an appointment.
                       - `"General"` → If the query does not fit the above categories.
                            - Use past conversation history (historyText) to generate a more relavent response.
                            - If the query is related to past messages, respond accordingly.
                            - If there is no relavent past context, generate a general response.

                    2. **Response Format:**
                       - Ensure that the response is always in **valid JSON** format.
                       - For non-General queries, set `"response": null`.
                       - Do not include extra explanations or text outside the JSON response.
                        
                    ### Examples:

                    #### Example 1: Login.
                    **User Query:** *"I forgot my password."*  
                    **Expected JSON Response:**
                    {
                      "intent": "Login",
                      "response": null
                    }

                    ### Examplae 2 : Schedule
                    **User Query: ** *"I want to book an appointment."*
                    **Expected JSON Response:**
                    {
                      "intent": "Schedule",
                      "response": null
                    }

                    ### Examplae 3 : Prescription
                    **User Query: ** *"I want to see my medical prescriptions"*
                    **Expected JSON Response:**
                    {
                      "intent": "Prescription",
                      "response": null
                    }

                    ### Examplae 3 : Payment
                    **User Query: ** *"Shoow my payment details"*
                    **Expected JSON Response:**
                    {
                      "intent": "Payment",
                      "response": null
                    }

                    ### Examplae 3 : Insurance
                    **User Query: ** *"Show insurance details"*
                    **Expected JSON Response:**
                    {
                      "intent": "Insurance",
                      "response": null
                    }

                    ### Example 3 : General Query with history
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
                      "intent": "General",
                      "response": "Your fever and cough will improve with proper rest, plenty of fluids, and following your doctor's advice."
                    }

                    ### Example 4 : General Query without history
                    **User Query: ** *"I have a fever and cough."*
                    **Expected JSON Response:**
                    {
                      "intent": "General",
                      "response": "You should take rest and drink plenty of fluids. If symptoms persist, consult a doctor."
                    }                    
                    """;

            var requestBody = new
            {
                model = "meta-llama/llama-3.3-70b-instruct:free",
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
                stream = false
            };

            httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await httpClient.PostAsJsonAsync(OpenRouterBaseUrl, requestBody);
            if (response.IsSuccessStatusCode)
            {
                var rawResponse = await response.Content.ReadAsStringAsync();
                var responseJson = JsonDocument.Parse(rawResponse);
                var contentProperty = responseJson.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return contentProperty ?? "No response content available.";
            }
            else
            {
                return "I am sorry, I could not understand your query. Please try again.";
            }
        }
    }
}
