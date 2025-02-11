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
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IConfiguration configuration;
        private const string OpenRouterBaseUrl = "https://openrouter.ai/api/v1/chat/completions";
        private string historyText;

        public GeneralQueryService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public async Task<string> GeneralQuery(int sessionId, string query)
        {
            var httpClient = httpClientFactory.CreateClient();
            var prompt = $$"""
                You are an advanced AI model that classifies user queries and extracts relevant entities. Your task is to analyze the given query and return a JSON response in the following format:

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
                   - `"General Query"` → If the query does not fit the above categories.
                     - Provide a relevant `"response"` based on your knowledge.

                2. **Extract Entities (if applicable):**
                   - `"email"` → Extract the email address if the query is a Login Query.
                   - `"otp"` → Extract a 6-digit OTP if present.
                   - `"date"` and `"time"` → If the query contains a date and/or time, extract and format them as:
                     - `"DD/MM/YY"` for date
                     - `"HH:MM AM/PM"` for time
                   - For the values that are not present, set them as `null`.
                3. **Response Format:**
                   - Ensure that the response is always in **valid JSON format**.
                   - For non-General queries, set `"response": null`.
                   - Do not include extra explanations or text outside the JSON response.

                ### Examples:

                #### Example 1: Login Query with Email  
                **User Query:** *"I forgot my password. My email is john.doe@example.com."*  
                **Response:**
                ```json
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
                **Response:**
                ```json
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
                **Response:**
                ```json
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

                ### Example 4 : General Query
                **User Query: ** *"What are the symptoms of flu?"*
                **Response:**
                ```json
                {
                  "intent": "General Query",
                  "entities": {
                    "email": null,
                    "otp": null,
                    "date": null,
                    "time": null
                  },
                  "response": "Common flu symptoms include fever, cough, sore throat, body aches, and fatigue."
                }

                Note  : provide only the json output in format as instructed. Do not include extra explanations or text outside the JSON response.
                
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

