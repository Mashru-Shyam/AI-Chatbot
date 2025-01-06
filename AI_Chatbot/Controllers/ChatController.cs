using System.Net.Http;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using AI_Chatbot.DTOs;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient httpClient;
        private readonly IConfiguration configuration;

        public ChatController(HttpClient httpClient, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.configuration = configuration;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> GetLlama3Response([FromBody] ChatRequestDto chatRequest)
        {
            var apiUrl = "https://openrouter.ai/api/v1/chat/completions";
            var apiKey = configuration["API:Key"];

            // Create request content with messages
            var requestContent = new
            {
                messages = new[]
                {
                    new { role = "user", content = chatRequest.Message }
                },
                max_tokens = 150
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestContent), Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await httpClient.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                // Read the response content as a string
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize response to extract the assistant's message
                var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var assistantMessage = responseJson
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (!string.IsNullOrEmpty(assistantMessage))
                {
                    return Ok(assistantMessage); // Return the message as plain text
                }
                else
                {
                    return StatusCode(500, "Response content is empty.");
                }
            }
            else
            {
                // Log the error response content
                var errorContent = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, errorContent);
            }
        }
    }
}
