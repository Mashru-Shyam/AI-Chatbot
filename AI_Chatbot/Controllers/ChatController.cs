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
        private readonly IEntityExtractionService extractionService;

        public ChatController(IChatService service, IGeneralQueryService queryService, IEntityExtractionService extractionService)
        {
            this.service = service;
            this.queryService = queryService;
            this.extractionService = extractionService;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto chatRequest)
        {
            if (chatRequest == null || string.IsNullOrEmpty(chatRequest.Message))
            {
                return BadRequest("Invalid request...");
            }

            var response = await service.Chatting(chatRequest);

            switch (response.ToString())
            {
                case "0":
                    var prescriptionEntities = extractionService.ExtractEntities(chatRequest);
                    return Ok(new { prescriptionEntities });
                case "1":
                    var answer = await queryService.GeneralQuery(chatRequest);
                    return Ok(new { answer });
                case "2":
                    var gAppointmentEntities = extractionService.ExtractEntities(chatRequest);
                    return Ok(new { gAppointmentEntities });
                case "3":
                    var sAppointmentEntities = extractionService.ExtractEntities(chatRequest);
                    return Ok(new { sAppointmentEntities});
                case "4":
                    var loginEntities = extractionService.ExtractEntities(chatRequest);
                    return Ok(new { loginEntities });
                case "5":
                    var insuranceEntities = extractionService.ExtractEntities(chatRequest);
                    return Ok(new { insuranceEntities });
                case "6":
                    var paymentEntities = extractionService.ExtractEntities(chatRequest);
                    return Ok(new { paymentEntities });
                default:
                    return Ok(response);
            }
        }
    }
}
