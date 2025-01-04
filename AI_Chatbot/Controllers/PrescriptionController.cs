using AI_Chatbot.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly IPrescriptionService service;

        public PrescriptionController(IPrescriptionService service)
        {
            this.service = service;
        }

        [HttpGet("get-prescriptions")]
        public async Task<IActionResult> GetPrescriptions()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid or missing user...");
            }
            var prescriptions = await service.GetPrescriptions(userId);
            return Ok(prescriptions);
        }
    }
}
