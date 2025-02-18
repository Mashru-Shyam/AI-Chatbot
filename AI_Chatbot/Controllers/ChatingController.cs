using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatingController : ControllerBase
    {
        private readonly IGeneralQueryService queryService;
        private readonly ILoginService loginService;
        private readonly IOtpService otpService;
        private readonly IConversationService conversationService;
        private readonly IPaymentService paymentService;
        private readonly IInsuranceService insuranceService;
        private readonly IPrescriptionService prescriptionService;
        private readonly IAppointmentService appointmentService;
        private readonly IChatHistoryService chatHistory;
        private string date;
        private string time;

        public ChatingController(IGeneralQueryService queryService, ILoginService loginService, IOtpService otpService, IConversationService conversationService,
            IPaymentService paymentService, IInsuranceService insuranceService, IPrescriptionService prescriptionService, IAppointmentService appointmentService, IChatHistoryService chatHistory)
        {
            this.queryService = queryService;
            this.loginService = loginService;
            this.otpService = otpService;
            this.conversationService = conversationService;
            this.paymentService = paymentService;
            this.insuranceService = insuranceService;
            this.prescriptionService = prescriptionService;
            this.appointmentService = appointmentService;
            this.chatHistory = chatHistory;
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
                var sessionId = session.GetHashCode();

                var result = await HandleClassification(sessionId: sessionId, query: query);
                if (result == "Enter Date and Time")
                {
                    return Ok(new { dateTime = result });
                }
                if (result == "Enter your query.")
                {
                    return Ok(new { query = result });
                }
                if (result == "Invalid Otp. Try again." || result == "Your OTP has been sent to your registered email address. Enter the Otp" || result == "Provide Email" || result == "Please login to continue")
                {
                    return Ok(new { otpEmail = result });
                }
                return Ok(result);
            }
            else
            {
                var sessionId = session.GetHashCode();
                var conversation = await conversationService.GetConversationAsync(sessionId);

                if (conversation.ConversationId == 0 || conversation.IsCompleted)
                {
                    var result = await HandleClassification(sessionId: sessionId, query: query);
                    if (result == "Enter Date and Time")
                    {
                        return Ok(new { dateTime = result });
                    }
                    if (result == "Enter your query.")
                    {
                        return Ok(new { query = result });
                    }
                    if (result == "Invalid Otp. Try again." || result == "Your OTP has been sent to your registered email address. Enter the Otp" || result == "Provide Email." || result == "Please login to continue")
                    {
                        return Ok(new { otpEmail = result });
                    }
                    return Ok(result);
                }

                else
                {
                    var result = await HandleFragmentation(sessionId: sessionId, context: conversation?.Context ?? "start", query: query);
                    if (result == "Enter Date and Time")
                    {
                        return Ok(new { dateTime = result });
                    }
                    if (result == "Enter your query.")
                    {
                        return Ok(new { query = result });
                    }
                    if (result == "Invalid Otp. Try again." || result == "Your OTP has been sent to your registered email address. Enter the Otp" || result == "Provide Email" ||result == "Please login to continue")
                    {
                        return Ok(new { otpEmail = result });
                    }
                    return Ok(result);
                }
            }
        }

        private async Task<string> HandleClassification(int sessionId, string query)
        {
            var answer = await queryService.GeneralQuery(sessionId, query);
            var resp = JsonDocument.Parse(answer);
            var intent = resp.RootElement.GetProperty("intent").GetString();

            switch (intent)
            {
                case "General Query":
                    var response = resp.RootElement.GetProperty("response").GetString();
                    if (response != null)
                    {
                        await chatHistory.AddChatHistory(sessionId, query, response);
                        return response;
                    }
                    return "Unable to process the query. Enter query again.";

                case "Login Query":
                    var emailEntityJson = resp.RootElement.GetProperty("entities")
                                                      .GetProperty("email");
                    if (emailEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var emailEntity = emailEntityJson.GetString();
                        var result = await HandleLogin(sessionId: sessionId, email: emailEntity);
                        return result;
                    }
                    await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "email");
                    return "Provide the Email";                    
                default:
                    await conversationService.UpdateConversationAsync(sessionId, intent: intent);
                    Request.Cookies.TryGetValue("Token", out var token);
                    if (token == null)
                    {
                        await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "email");
                        return "Please login to continue";

                    }
                    var res = await HandleIntent(sessionId: sessionId, intent: intent, entities: null, token: token);
                    return res;
            }
        }

        private async Task<string> HandleFragmentation(int sessionId, string context, string query)
        {
            var answer = await queryService.GeneralQuery(sessionId: sessionId, query: query);
            var resp = JsonDocument.Parse(answer);

            switch (context)
            {
                case "otp":
                    var otpEntityJson = resp.RootElement.GetProperty("entities")
                                                      .GetProperty("otp");
                    if (otpEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var otpEntity = otpEntityJson.GetString();
                        var result = await otpService.CheckOtp(otpEntity);
                        if (string.IsNullOrEmpty(result))
                        {
                            return "Invalid Otp. Try again.";
                        }
                        Response.Cookies.Append("Token", result, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTime.UtcNow.AddDays(1)
                        });

                        var conversation = await conversationService.GetConversationAsync(sessionId);
                        if (conversation.Intent != "none")
                        {
                            var data = await HandleIntent(sessionId: sessionId, intent: conversation.Intent, entities: conversation.Entities.ToList(), token: result);
                            return data;
                        }
                        else
                        {
                            await conversationService.UpdateConversationAsync(sessionId: sessionId, IsCompleted: true, status: "end");
                            await conversationService.DeleteEntitiesAsync(sessionId: sessionId);
                            return "Enter your query.";
                        }
                    }
                    return "Invalid Otp. Try again.";

                case "email":
                    var emailEntityJson = resp.RootElement.GetProperty("entities")
                                                      .GetProperty("email");
                    if (emailEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var emailEntity = emailEntityJson.GetString();
                        var result = await HandleLogin(sessionId: sessionId, email: emailEntity);
                        return result;
                    }
                    await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "email");
                    return "Provide the Email";
                case "date-time":
                    var dateEntityJson = resp.RootElement.GetProperty("entities")
                                                      .GetProperty("date");
                    var timeEntityJson = resp.RootElement.GetProperty("entities")
                                                        .GetProperty("time");
                    if(dateEntityJson.ValueKind != JsonValueKind.Null || timeEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var dateEntity = dateEntityJson.GetString();
                        var timeEntity = timeEntityJson.GetString();
                        var dateTimeResponse = await HandleDateTime(sessionId: sessionId, date: dateEntity, time: timeEntity);
                        return dateTimeResponse;
                    }
                    return "Enter Date and Time";
                default:
                    return "Enter your query.";
            }
        }

        private async Task<string> HandleLogin(int sessionId, string email)
        {
            var entities = new List<Entity>
            {
                new Entity { EntityName = "email", EntityValue = email}
            };
            await conversationService.UpdateConversationAsync(sessionId: sessionId, entities: entities);
            var user = await loginService.GetUser(email);   
            if (user == 0)
            {
                await loginService.AddUser(email);
            }
            var otp = otpService.GenerateOtp();
            await otpService.SendOtpViaMail(to: email, subject: "Your OTP Code", body: $"Thank you for using our service. Your one-time password (OTP) to access your account is:\r\n\r\n{otp}\r\n\r\n Please note that this OTP is valid for a limited time (e.g., 5 minutes). Do not share this code with anyone. If you did not request this OTP, please ignore this message.\r\n");
            await otpService.StoreOtp(email: email, otp: otp);
            await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "otp");
            await conversationService.DeleteEntitiesAsync(sessionId: sessionId);
            return "Your OTP has been sent to your registered email address. Enter the Otp";
        }

        private async Task<string> HandleIntent(int sessionId, string intent, List<Entity>? entities, string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdClaim ?? throw new ArgumentNullException(nameof(userIdClaim)));

            switch (intent)
            {
                case "View Appointment Query":
                    var appointments = await appointmentService.GetAppointments(userId);
                    string appointment = string.Join("\n\n", appointments.Select(a =>
                        $"**Date**: {a.AppointmentDate} \n **Time**: {a.AppointmentTime}"));

                    await conversationService.UpdateConversationAsync(sessionId: sessionId, IsCompleted: true, status: "end");
                    return "Your appointments are at: \n\n" + appointment;

                case "Book Appointment Query":
                    var conversation = await conversationService.GetConversationAsync(sessionId);
                    var dateEntity = conversation.Entities?.FirstOrDefault(e => e.EntityName == "date");
                    var timeEntity = conversation.Entities?.FirstOrDefault(e => e.EntityName == "time");

                    date = dateEntity?.EntityValue ?? string.Empty;
                    time = timeEntity?.EntityValue ?? string.Empty;
                    if (date == string.Empty || time == string.Empty)
                    {
                        await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "date-time");
                        return "Enter Date and Time";
                    }
                    var appointmentDto = new AppointmentDto
                    {
                        AppointmentDate = date,
                        AppointmentTime = time
                    };
                    var schedules = await appointmentService.AddAppointment(userId: userId, appointmentDto: appointmentDto);

                    await conversationService.UpdateConversationAsync(sessionId : sessionId, IsCompleted: true, status: "end");
                    await conversationService.DeleteEntitiesAsync(sessionId: sessionId);
                    return schedules;

                case "View Prescription Query":
                    var prescriptions = await prescriptionService.GetPrescriptions(userId);
                    string prescription = string.Join("\n\n", prescriptions.Select(p =>
                        $"**Medicine Name**: {p.MedicineName} \n **Medicine Dosage**: {p.MedicineDosage} \n **Medicine Direction**: {p.MedicineDirection}"));

                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return "Your prescription are: \n\n" + prescription;

                case "View Insurance Query":
                    var insuranceDetails = await insuranceService.GetInsuranceDetails(userId);
                    string insurance = string.Join("\n\n", insuranceDetails.Select(i =>
                        $"**Insurance Name **: {i.InsuranceName} \n **Start Date**: {i.InsuranceStart} \n **End Date**: {i.InsuranceEnd}, \n **Status**: {i.InsuranceStatus}"));

                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return "Your insurance with their details are: \n\n" + insurance;

                case "View Payments Query":
                    var payments = await paymentService.GetDuePayments(userId);
                    string payment = string.Join("\n\n", payments.Select(p =>
                        $"**Payment Due**: {p.PaymentDue} \n **Amount**: Rs {p.PaymentAmount} \n **Status**: {p.PaymentStatus}"));

                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "end");
                    return "Your payment details are: \n\n" + payment;

                default:
                    return "Enter your query.";
            }
        }

        private async Task<string> HandleDateTime(int sessionId, string date, string time)
        {
            

            var entities = new List<Entity>
            {
                new Entity { EntityName = "date", EntityValue = date },
                new Entity { EntityName = "time", EntityValue = time }
            };
            await conversationService.UpdateConversationAsync(sessionId, entities: entities);
            var con = await conversationService.GetConversationAsync(sessionId);
            Request.Cookies.TryGetValue("Token", out var token);
            if (token == null)
            {
                await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "email");
                return "Provide your email.";
            }
            return await HandleIntent(sessionId: sessionId, intent: con.Intent, entities: entities, token: token);
        }
    }
}
