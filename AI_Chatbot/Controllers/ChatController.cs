using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService service;

        public ChatController(IChatService service)
        {
            this.service = service;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto chatRequest)
        {
            if(chatRequest == null || string.IsNullOrEmpty(chatRequest.Message))
            {
                return BadRequest("Invalid request...");
            }

            var response = await service.Chatting(chatRequest);
            return Ok(response);
        }
    }
}
