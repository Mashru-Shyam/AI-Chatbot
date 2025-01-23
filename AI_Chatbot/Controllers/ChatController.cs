using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Migrations;
using AI_Chatbot.Models;
using AI_Chatbot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IIntentClassificationService intentClassificationService;
        private readonly IGeneralQueryService queryService;
        private readonly IEntityExtractionService extractionService;
        private readonly ILoginService loginService;
        private readonly IRegisterService registerService;
        private readonly IOtpService otpService;
        private readonly IConversationService conversationService;
        private readonly IPaymentService paymentService;
        private readonly IInsuranceService insuranceService;
        private readonly IPrescriptionService prescriptionService;
        private readonly IAppointmentService appointmentService;
        private string intent;
        private ICollection<Entity> entities;

        public ChatController(IIntentClassificationService intentClassificationService, IGeneralQueryService queryService, IEntityExtractionService extractionService, ILoginService loginService, IRegisterService registerService, IOtpService otpService, IConversationService conversationService, IPaymentService paymentService, IInsuranceService insuranceService, IPrescriptionService prescriptionService, IAppointmentService appointmentService)
        {
            this.intentClassificationService = intentClassificationService;
            this.queryService = queryService;
            this.extractionService = extractionService;
            this.loginService = loginService;
            this.registerService = registerService;
            this.otpService = otpService;
            this.conversationService = conversationService;
            this.paymentService = paymentService;
            this.insuranceService = insuranceService;
            this.prescriptionService = prescriptionService;
            this.appointmentService = appointmentService;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] string query)
        {
            if (query == null)
            {
                return Ok("Please provide the message...");
            }

            var sessionId = GetSessionId();

            var i = await intentClassificationService.Chatting(query);

            if (i.ToString() == "1")
            {
                var answer = await queryService.GeneralQuery(query);
                return Ok(answer);
            }

            var extractedEntities = extractionService.ExtractEntities(query);
            entities = ConvertToEntityCollection(extractedEntities);

            if (i.ToString() == "5")
            {
                return Ok(await HandleLogin());
            }
            else if(i.ToString() == "4")
            {
                var result = await HandleOtp();
                if (result == "null")
                {
                    return Ok("Provide valid OTP...");
                }
                return Ok(new { token = result });
            }

            var con = await conversationService.GetConversationAsync(sessionId);
            if (con == null || con.IsCompleted == true)
            {
                intent = GetIntent(i);
                await conversationService.UpdateConversationAsync(sessionId, intent: intent, entities: entities, false, "start");
            }
            else {
                intent = con.Intent;
                entities = con.Entities;
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Ok("For this query you need to login \n Please provide your email...");
            }

            switch (intent)
            {
                case "gAppointment":
                    var appointments = await appointmentService.GetAppointments(userId);
                    string appointment= string.Join("\n\n", appointments.Select(a =>
                        $"**Date**: {a.AppointmentDate} \n **Time**: {a.AppointmentTime}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return Ok(appointment);
                case "sAppointment":
                    return Ok("sAppointment");
                case "prescription":
                    var prescriptions = await prescriptionService.GetPrescriptions(userId);
                    string prescription = string.Join("\n\n", prescriptions.Select(p =>
                        $"**Medicine Name**: {p.MedicineName} \n **Medicine Dosage**: {p.MedicineDosage} \n **Medicine Direction**: {p.MedicineDirection}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return Ok(prescription);
                case "insurance":
                    var insuranceDetails = await insuranceService.GetInsuranceDetails(userId);
                    string insurance = string.Join("\n\n", insuranceDetails.Select(i =>
                        $"**Insurance Name **: {i.InsuranceName} \n **Start Date**: {i.InsuranceStart} \n **End Date**: {i.InsuranceEnd}, \n **Status**: {i.InsuranceStatus}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return Ok(insurance);
                case "payment":
                    var payments = await paymentService.GetDuePayments(userId);
                    string payment = string.Join("\n\n", payments.Select(p =>
                        $"**Payment Due**: {p.PaymentDue} \n **Amount**: {p.PaymentAmount} \n **Status**: {p.PaymentStatus}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return Ok(payment);
            }
            return Ok(".");
        }

        private async Task<string> HandleOtp()
        {
            var otpEntity = entities.FirstOrDefault(e => e.EntityName == "otp");
            if (otpEntity == null)
            {
                return "null";
            }
            var result = await otpService.CheckOtp(otpEntity.EntityValue);
            if (result == null)
            {
                return "null";
            }
            return result;
        }

        private async Task<string> HandleLogin()
        {
            var emailEntity = entities.FirstOrDefault(e => e.EntityName == "email");
            if (emailEntity == null)
            {
                return "Please provide your email address...";
            }

            var user = await loginService.CheckUser(emailEntity.EntityValue);
            if (user == 0)
            {
                await registerService.Register(emailEntity.EntityValue);
            }
            var otp = otpService.GenerateOtp();
            await otpService.StoreOtp(emailEntity.EntityValue, otp);
            await otpService.SendOtpViaMail(emailEntity.EntityValue, "OTP for Login", $"Your OTP is {otp}");
            return "OTP sent to your email.\nProvide the OTP...";
        }

        private string GetIntent(string i)
        {
            switch (i)
            {
                case "0":
                    return "prescription";
                case "2":
                    return "gAppointment";
                case "3":
                    return "sAppointment";
                case "6":
                    return "insurance";
                case "7":
                    return "payment";
                default:
                    return "general";
            }
        }

        private int GetSessionId()
        {
                if (!Request.Cookies.TryGetValue("SessionId", out var sessionId))
                {
                    sessionId = Guid.NewGuid().ToString();
                    Response.Cookies.Append("SessionId", sessionId, new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddDays(1),
                        HttpOnly = true,
                        Secure = true
                    });
                }
                return sessionId.GetHashCode();
        }

        private ICollection<Entity> ConvertToEntityCollection(Dictionary<string, List<string>> extractedEntities)
        {
            var entities = new List<Entity>();

            foreach (var kvp in extractedEntities)
            {
                foreach (var value in kvp.Value)
                {
                    entities.Add(new Entity
                    {
                        EntityName = kvp.Key,
                        EntityValue = value
                    });
                }
            }

            return entities;
        }
    }
}