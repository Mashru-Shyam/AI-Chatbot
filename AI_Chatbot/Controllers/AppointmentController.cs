using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService service;

        public AppointmentController(IAppointmentService service)
        {
            this.service = service;
        }

        [HttpPost("set-appointment")]
        public async Task<IActionResult> AddAppointment([FromBody] AppointmentDto appointmentDto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid or missing user...");
            }

            var message = await service.AddAppointment(userId, appointmentDto);
            return Ok(message);
        }

        [HttpGet("get-appointment")]
        public async Task<IActionResult> GetAppointments()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid or missing user...");
            }
            var appointments = await service.GetAppointments(userId);
            return Ok(appointments);
        }
    }
}
