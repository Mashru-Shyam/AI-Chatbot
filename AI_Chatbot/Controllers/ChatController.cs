using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Migrations;
using AI_Chatbot.Models;
using AI_Chatbot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly IConversationService conversationService;

        public ChatController(IIntentClassificationService service, IGeneralQueryService queryService, IEntityExtractionService extractionService, ILoginService loginService, IRegisterService registerService, IOtpService otpService, IConversationService conversationService)
        {
            this.service = service;
            this.queryService = queryService;
            this.extractionService = extractionService;
            this.loginService = loginService;
            this.registerService = registerService;
            this.otpService = otpService;
            this.conversationService = conversationService;
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
                        return Ok("Email extraction failed.\nPlease provide a valid email.");
                    }

                    var user = await loginService.CheckUser(email);
                    if (user == 0)
                    {
                        await registerService.Register(email);
                    }
                    var otp = otpService.GenerateOtp();
                    await otpService.StoreOtp(email, otp);
                    await otpService.SendOtpViaMail(email, "OTP for Login", $"Your OTP is {otp}");
                    await conversationService.AddConversationAsync(
                            userId: user,
                            intent: "login",
                            entities: new List<Entity>
                            {
                                new Entity { EntityName = "email", EntityValue= email },
                            },
                            IsCompleted: true,
                            status: "Otp Collection"
                        );
                    return Ok("OTP sent to your email.\nProvide the OTP");
                }
                else if (intent.ToString() == "7")
                {
                    var entities = extractionService.ExtractEntities(chatRequest);
                    if (!entities["otp"].Any())
                    {
                        return Ok("Provide your valid otp...");
                    }
                    var otp = entities["otp"].FirstOrDefault();
                    if (otp == null)
                    {
                        return Ok("Otp extraction failed.\nPlease provide a valid otp.");
                    }

                    var result = await otpService.CheckOtp(otp);
                    if (result == null)
                    {
                        return Ok("Invalid OTP...");
                    }
                    return Ok(new { token = result });
                }
                else
                {
                    return Ok("For processing the query login is required\nProvide your email to login...");
                }
            }
            return Ok(userId);
        }
    }
}