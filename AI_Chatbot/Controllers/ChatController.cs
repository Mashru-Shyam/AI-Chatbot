using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Migrations;
using AI_Chatbot.Models;
using AI_Chatbot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IGeneralQueryService queryService;
        private readonly ILoginService loginService;
        private readonly IOtpService otpService;
        private readonly IConversationService conversationService;
        private readonly IPaymentService paymentService;
        private readonly IInsuranceService insuranceService;
        private readonly IPrescriptionService prescriptionService;
        private readonly IAppointmentService appointmentService;
        private string date;
        private string otp;
        private string email;
        private string time;

        public ChatController(
            IGeneralQueryService queryService,
            ILoginService loginService,
            IOtpService otpService,
            IConversationService conversationService,
            IPaymentService paymentService,
            IInsuranceService insuranceService,
            IPrescriptionService prescriptionService,
            IAppointmentService appointmentService
            )
        {
            this.queryService = queryService;
            this.loginService = loginService;
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
            Request.Cookies.TryGetValue("SessionId", out var session);
            if (session == null)
            {
                session = Guid.NewGuid().ToString();
                Response.Cookies.Append("SessionId", session, new CookieOptions
                {
                    Secure = true,
                    HttpOnly = true,
                    SameSite = SameSiteMode.None
                });
                var sessionI = session.GetHashCode();
                var result = await HandleClassification(sessionI, query);
                return Ok(result);
            }
            var sessionId = session.GetHashCode();
            var conversation = await conversationService.GetConversationAsync(sessionId);
            if (conversation == null || conversation.IsCompleted)
            {
                var result = await HandleClassification(sessionId, query);
                return Ok(result);
            }
            else
            {
                var result = await HandleFragmentation(sessionId, conversation.Context, query);
                return Ok(result);
            }
        }
        private async Task<string> HandleFragmentation(int sessionId, string context, string query)
        {
            switch(context)
            {
                case "otp":
                    var result = await otpService.CheckOtp(query);
                    if (result == null)
                    {
                        return "null";
                    }
                    Response.Cookies.Append("Token", result, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None
                    });
                    var conversation = await conversationService.GetConversationAsync(sessionId);
                    if (conversation.Intent != null) {
                        var res = await HandleIntent(sessionId,conversation.Intent, conversation.Entities.ToList(),result);
                        return res;
                    }
                    else
                    {
                        await conversationService.UpdateConversationAsync(sessionId,IsCompleted:true, status:"end");
                        return "Login Sucessfull. Provide the query.";
                    }
                case "login":
                    var response = await HandleLogin(sessionId, query);
                    return response;
                case "date-time":
                    string datePattern = @"\b(?:\d{1,2}(?:st|nd|rd|th)?(?:\s)?(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|January|February|March|April|May|June|July|August|September|October|November|December)\s?\d{0,4})|\b(?:\d{4}-\d{2}-\d{2})\b"; // Date regex
                    string timePattern = @"\b(?:[01]?\d|2[0-3]):[0-5]\d(?:\s?(?:AM|PM|am|pm))?\b|\b(?:[01]?\d|2[0-3])(?:\s?(?:AM|PM|am|pm))\b"; // Time regex
                    date = Regex.Match(query, datePattern).Value;
                    time = Regex.Match(query, timePattern).Value;
                    var entities = new List<Entity>
                    {
                            new Entity { EntityName = "date", EntityValue = date },
                            new Entity { EntityName = "time", EntityValue = time }
                    };
                    await conversationService.UpdateConversationAsync(sessionId, entities: entities);
                    var conv = await conversationService.GetConversationAsync(sessionId);
                    Request.Cookies.TryGetValue("Token", out var token);
                    return await HandleIntent(sessionId, conv.Intent, entities, token);
                default:
                    return ".";
            }
        }

        private async Task<string> HandleClassification(int sessionId, string query)
        {
            var answer = await queryService.GeneralQuery(query);
            var resp = JsonDocument.Parse(answer);
            var intent = resp.RootElement.GetProperty("intent").GetString();

            switch (intent)
            {
                case "General":
                    var response = resp.RootElement.GetProperty("response").GetString();
                    if (response != null)
                    {
                        return response;
                    }
                    return "Unable to process the query";
                case "Login":
                    var loginResult = await HandleLogin(sessionId, query);
                    if (loginResult != null)
                    {
                        return loginResult;
                    }
                    return "Unable to process the query";
                default:
                    string datePattern = @"\b(?:\d{1,2}(?:st|nd|rd|th)?(?:\s)?(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec|January|February|March|April|May|June|July|August|September|October|November|December)\s?\d{0,4})|\b(?:\d{4}-\d{2}-\d{2})\b"; // Date regex
                    string timePattern = @"\b(?:[01]?\d|2[0-3]):[0-5]\d(?:\s?(?:AM|PM|am|pm))?\b|\b(?:[01]?\d|2[0-3])(?:\s?(?:AM|PM|am|pm))\b"; // Time regex
                    date = Regex.Match(query, datePattern).Value;
                    time = Regex.Match(query, timePattern).Value;
                    var entities = new List<Entity>
                    {
                            new Entity { EntityName = "date", EntityValue = date },
                            new Entity { EntityName = "time", EntityValue = time }
                    };
                    await conversationService.UpdateConversationAsync(sessionId, intent: intent, entities: entities);
                    Request.Cookies.TryGetValue("Token", out var token);
                    if(token!=null)
                    {
                        var result = await HandleIntent(sessionId, intent, entities, token);
                        return result;
                    }
                    else
                    {
                        var result = await HandleLogin(sessionId, query);
                        return result;
                    }
            }
        }

        private async Task<string> HandleIntent(int sessionId, string intent, List<Entity> entities ,string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdClaim ?? throw new ArgumentNullException(nameof(userIdClaim)));

            switch (intent)
            {
                case "gAppointment":
                    var appointments = await appointmentService.GetAppointments(userId);
                    string appointment = string.Join("\n\n", appointments.Select(a =>
                        $"**Date**: {a.AppointmentDate} \n **Time**: {a.AppointmentTime}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return appointment;
                case "sAppointment":
                    var conversation = await conversationService.GetConversationAsync(sessionId);
                    var dateEntity = conversation.Entities?.FirstOrDefault(e => e.EntityName == "date");
                    var timeEntity = conversation.Entities?.FirstOrDefault(e => e.EntityName == "time");

                    date = dateEntity?.EntityValue ?? string.Empty;
                    time = timeEntity?.EntityValue ?? string.Empty;
                    if (date == string.Empty || time == string.Empty)
                    {
                        await conversationService.UpdateConversationAsync(sessionId, status: "date-time", IsCompleted: false);
                        return "Provide the of Appointment as \n **Date** : **YYYY-MM-DD**) and **Time** : **HH:MM am/pm**)";
                    }
                    var appointmentDto = new AppointmentDto
                    {
                        AppointmentDate = DateOnly.Parse(date),
                        AppointmentTime = TimeOnly.Parse(time)
                    };
                    var app = await appointmentService.AddAppointment(userId, appointmentDto);
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return app;
                case "Prescriptions":
                    var prescriptions = await prescriptionService.GetPrescriptions(userId);
                    string prescription = string.Join("\n\n", prescriptions.Select(p =>
                        $"**Medicine Name**: {p.MedicineName} \n **Medicine Dosage**: {p.MedicineDosage} \n **Medicine Direction**: {p.MedicineDirection}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return prescription;
                case "Insurance":
                    var insuranceDetails = await insuranceService.GetInsuranceDetails(userId);
                    string insurance = string.Join("\n\n", insuranceDetails.Select(i =>
                        $"**Insurance Name **: {i.InsuranceName} \n **Start Date**: {i.InsuranceStart} \n **End Date**: {i.InsuranceEnd}, \n **Status**: {i.InsuranceStatus}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return insurance;
                case "Payment":
                    var payments = await paymentService.GetDuePayments(userId);
                    string payment = string.Join("\n\n", payments.Select(p =>
                        $"**Payment Due**: {p.PaymentDue} \n **Amount**: {p.PaymentAmount} \n **Status**: {p.PaymentStatus}"));
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return payment;
                default:
                    return intent;
            }
        }

        private async Task<string> HandleLogin(int sessionId, string query)
        {
            string emailPattern = @"[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+";
            email = Regex.Match(query, emailPattern).Value;
            if (email == "")
            {
                await conversationService.UpdateConversationAsync(sessionId, status: "login");
                return "Please provide your email address...";
            }
            var entities = new List<Entity>
            {
                new Entity { EntityName = "email", EntityValue = email}
            };
            await conversationService.UpdateConversationAsync(sessionId, entities: entities);
            var user = await loginService.GetUser(email);
            if (!user)
            {
                await loginService.AddUser(email);
            }
            var otp = otpService.GenerateOtp();
            await otpService.SendOtpViaMail(email, "Your OTP Code", $"Thank you for using our service. Your one-time password (OTP) to access your account is:\r\n\r\n{otp}\r\n\r\n Please note that this OTP is valid for a limited time (e.g., 5 minutes). Do not share this code with anyone. If you did not request this OTP, please ignore this message.\r\n");
            await otpService.StoreOtp(email, otp);
            await conversationService.UpdateConversationAsync(sessionId, status: "otp");
            return "Your OTP has been sent to your registered email address. Enter the Otp";
        }
    }
}