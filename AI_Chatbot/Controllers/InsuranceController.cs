using AI_Chatbot.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceController : ControllerBase
    {
        private readonly IInsuranceService service;

        public InsuranceController(IInsuranceService service)
        {
            this.service = service;
        }

        [HttpGet("get-insurance-details")]
        public async Task<IActionResult> GetInsuranceDetails()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Invalid or missing user...");
            }
            var insuranceDetails = await service.GetInsuranceDetails(userId);
            return Ok(insuranceDetails);
        }
    }
}
