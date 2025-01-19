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
        private readonly IGeneralQueryService queryService;

        public ChatController(IChatService service, IGeneralQueryService queryService)
        {
            this.service = service;
            this.queryService = queryService;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto chatRequest)
        {
            if(chatRequest == null || string.IsNullOrEmpty(chatRequest.Message))
            {
                return BadRequest("Invalid request...");
            }

            var response = await service.Chatting(chatRequest);

            switch (response.ToString())
            {
                case "0":
                    return Ok("get-prescription");
                case "1":
                    var answer = await queryService.GeneralQuery(chatRequest);
                    return Ok(new { answer});
                case "2":
                    return Ok("get-appointment");
                case "3":
                    return Ok("set-appointment");
                case "4":
                    return Ok("login");
                case "5":
                    return Ok("get-insurance");
                case "6":
                    return Ok("get-payment");
                default:
                    return Ok(response);
            }
        }
    }
}
