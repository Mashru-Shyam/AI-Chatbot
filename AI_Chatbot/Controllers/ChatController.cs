using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Migrations;
using AI_Chatbot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IIntentClassificationService service;
        private readonly IGeneralQueryService queryService;
        private readonly IEntityExtractionService extractionService;
        private readonly ILoginService loginService;
        private readonly IRegisterService registerService;
        private readonly IOtpService otpService;

        public ChatController(IIntentClassificationService service, IGeneralQueryService queryService, IEntityExtractionService extractionService, ILoginService loginService, IRegisterService registerService, IOtpService otpService)
        {
            this.service = service;
            this.queryService = queryService;
            this.extractionService = extractionService;
            this.loginService = loginService;
            this.registerService = registerService;
            this.otpService = otpService;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequestDto chatRequest)
        {
            if (chatRequest == null || string.IsNullOrEmpty(chatRequest.Message))
            {
                return Ok("Please provide the message...");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdClaim, out var userId);

            if (userId == 0)
            {
                var intent = await service.Chatting(chatRequest);

                if (intent.ToString() == "1")
                {
                    var answer = await queryService.GeneralQuery(chatRequest);
                    return Ok(answer);
                }
                else if (intent.ToString() == "4")
                {
                    var entities = extractionService.ExtractEntities(chatRequest);
                    if (!entities["email"].Any())
                    {
                        return Ok("Provide your email to login...");
                    }

                    var email = entities["email"].FirstOrDefault();
                    if (email == null)
                    {
                        return Ok("Email extraction failed. Please provide a valid email.");
                    }

                    var user = await loginService.CheckUser(email);
                    if (!user)
                    {
                        await registerService.Register(email);
                    }
                    var otp = otpService.GenerateOtp();
                    await otpService.StoreOtp(email, otp);
                    await otpService.SendOtpViaMail(email, "OTP for Login", $"Your OTP is {otp}");
                    return Ok("OTP sent to your email. \n Provide the OTP");
                }
                else
                {
                    return Ok("For processing the query login is required \n Provide your email to login...");
                }
            }
            return Ok("Please provide the message...");
        }
    }
}