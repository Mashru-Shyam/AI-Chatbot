using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AI_Chatbot.Services
{
    public class IntentClassificationService : IIntentClassificationService
    {
        private readonly HttpClient httpClient;
        private const string OllamaBaseUrl = "http://localhost:11434/api/chat";

        public IntentClassificationService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<string> Chatting(string query)
        {
            var requestContent = new StringContent(JsonConvert.SerializeObject(new { text = query }), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("http://localhost:5000/classify", requestContent);
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

    }
}