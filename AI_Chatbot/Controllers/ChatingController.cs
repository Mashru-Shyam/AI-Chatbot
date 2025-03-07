using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AI_Chatbot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatingController : ControllerBase
    {
        private readonly ILoginService loginService;
        private readonly IOtpService otpService;
        private readonly IConversationService conversationService;
        private readonly IPaymentService paymentService;
        private readonly IInsuranceService insuranceService;
        private readonly IPrescriptionService prescriptionService;
        private readonly IAppointmentService appointmentService;
        private readonly IChatHistoryService chatHistory;
        private readonly IQueryService queryService;
        private string date;
        private string time;

        public ChatingController(ILoginService loginService, IOtpService otpService, IConversationService conversationService,
            IPaymentService paymentService, IInsuranceService insuranceService, IPrescriptionService prescriptionService, IAppointmentService appointmentService, IChatHistoryService chatHistory, IQueryService queryService)
        {
            this.loginService = loginService;
            this.otpService = otpService;
            this.conversationService = conversationService;
            this.paymentService = paymentService;
            this.insuranceService = insuranceService;
            this.prescriptionService = prescriptionService;
            this.appointmentService = appointmentService;
            this.chatHistory = chatHistory;
            this.queryService = queryService;
            this.date = string.Empty;
            this.time = string.Empty;
        }

        //Message sending endpoint
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] string query)
        {
            //Check if the session id is present in the cookies
            Request.Cookies.TryGetValue("SessionId", out var session);

            //If the session id is not present, create a new session id and store it in the cookies
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
                return ResponseFormatter(result);
            }

            //If the session id Present, check if the conversation is completed or not
            else
            {
                var sessionId = session.GetHashCode();
                var conversation = await conversationService.GetConversationAsync(sessionId);

                //If the conversation is completed or no conversations, classify the query
                if (conversation.ConversationId == 0 || conversation.IsCompleted)
                {
                    var result = await HandleClassification(sessionId: sessionId, query: query);
                    return ResponseFormatter(result);
                }

                //If the conversation is not completed, handle the fragmentation
                else
                {
                    var result = await HandleFragmentation(sessionId: sessionId, context: conversation?.Context ?? "Start", query: query);
                    return ResponseFormatter(result);
                }
            }
        }

        //Format the response for differnet queries
        private IActionResult ResponseFormatter(string result)
        {
            return result switch
            {
                "Please enter the date 📅" => Ok(new { date = result }),
                "Please enter the time ⏰" => Ok(new { time = result }),
                "Please type your query below ✍️" => Ok(new { query = result }),
                "Invalid OTP! Please try again. 🔄" or "OTP sent to your registered email. Please enter it below. 📩"
                or "Please enter your email 📧" => Ok(new { otpEmail = result }),
                _ => Ok(result)
            };
        }

        //Classify the query and handle the intent
        private async Task<string> HandleClassification(int sessionId, string query)
        {
            var answer = await queryService.IntentClassification(sessionId, query);
            var resp = JsonDocument.Parse(answer);
            var intent = resp.RootElement.GetProperty("intent").GetString();

            switch (intent)
            {
                //A General Query, No fragmentations, Direct response
                case "General":
                    var response = resp.RootElement.GetProperty("response").GetString();
                    if (response != null)
                    {
                        await chatHistory.AddChatHistory(sessionId, query, response);
                        return response;
                    }
                    return "Couldn't process your request. Please try again. 🔄";

                //A Login Query, Email Fragmentation, Ask for Email or Direct Login
                case "Login":
                    var entity = await queryService.EntityExtraction(sessionId, query);
                    var emailEntityJson = JsonDocument.Parse(entity);
                    var emailEntity = emailEntityJson.RootElement.GetProperty("entities")
                                                      .GetProperty("email");

                    if (emailEntity.ValueKind != JsonValueKind.Null)
                    {
                        var email = emailEntity.GetString();
                        var result = await HandleLogin(sessionId: sessionId, email: email ?? string.Empty);
                        return result;
                    }

                    //Update conversation to get email
                    await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Email");
                    return "Please enter your email 📧";
                
                //Other Queries
                default:
                    var values = await queryService.EntityExtraction(sessionId, query);
                    var entityJson = JsonDocument.Parse(values);
                    var dateEntity = entityJson.RootElement.GetProperty("entities")
                                        .GetProperty("date");
                    var timeEntity = entityJson.RootElement.GetProperty("entities")
                                        .GetProperty("time");
                    if (dateEntity.ValueKind != JsonValueKind.Null)
                    {
                        var date = dateEntity.GetString();
                        var entities = new List<Entity>
                        {
                            new Entity { EntityName = "Date", EntityValue = date},
                        };
                        //Update conversation with date entity
                        await conversationService.UpdateConversationAsync(sessionId, entities: entities);
                    }
                    if (timeEntity.ValueKind != JsonValueKind.Null)
                    {
                        var time = timeEntity.GetString();
                        var entities = new List<Entity>
                        {
                            new Entity { EntityName = "Time", EntityValue = time},
                        };
                        //Update conversation with date entity
                        await conversationService.UpdateConversationAsync(sessionId, entities: entities);
                    }
                    //Update conversation Intent
                    await conversationService.UpdateConversationAsync(sessionId, intent: intent ?? "None");
                    Request.Cookies.TryGetValue("Token", out var token);

                    if (token == null)
                    {
                        //Update Conversation to get email
                        await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Email");
                        return "Please enter your email 📧";
                    }

                    var con = await conversationService.GetConversationAsync(sessionId);
                    //Handle the intents of User Query
                    var res = await HandleIntent(sessionId: sessionId, intent: intent ?? "None", entities: con.Entities.ToList(), token: token);
                    return res;
            }
        }

        //Handle the incomplete parts of the query
        private async Task<string> HandleFragmentation(int sessionId, string context, string query)
        {
            var answer = await queryService.EntityExtraction(sessionId: sessionId, query: query);
            var resp = JsonDocument.Parse(answer);

            switch (context)
            {
                //Otp Provide by user
                case "Otp":
                    var otpEntityJson = resp.RootElement.GetProperty("entities")
                                                      .GetProperty("otp");
                    if (otpEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var otpEntity = otpEntityJson.GetString();
                        //Check for valid otp
                        var result = await otpService.CheckOtp(otpEntity ?? string.Empty);
                        if (string.IsNullOrEmpty(result))
                        {
                            return "Invalid OTP! Please try again. 🔄";
                        }
                        Response.Cookies.Append("Token", result, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Expires = DateTime.UtcNow.AddDays(1)
                        });

                        //Get the conversation before login
                        var conversation = await conversationService.GetConversationAsync(sessionId);
                        if (conversation.Intent != "None")
                        {
                            var data = await HandleIntent(sessionId: sessionId, intent: conversation.Intent, entities: conversation.Entities.ToList(), token: result);
                            return data;
                        }
                        else
                        {
                            //Update the conversation to end (Delete also if required)
                            await conversationService.UpdateConversationAsync(sessionId: sessionId, IsCompleted: true, status: "End");
                            return "Please type your query below ✍️";
                        }
                    }
                    return "Invalid OTP! Please try again. 🔄";
                //Email Provide by user
                case "Email":
                    var emailEntityJson = resp.RootElement.GetProperty("entities")
                                                      .GetProperty("email");
                    if (emailEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var emailEntity = emailEntityJson.GetString();
                        var result = await HandleLogin(sessionId: sessionId, email: emailEntity ?? string.Empty);
                        return result;
                    }

                    //Update conversation to get email
                    await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Email");
                    return "Please enter your email 📧";

                //Date Provided by uaer
                case "Date":
                    var dateEntityJson = resp.RootElement.GetProperty("entities")
                                                      .GetProperty("date");
                    
                    if (dateEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var dateEntity = dateEntityJson.GetString();
                        //Add Date Entity to Database
                        var entities = new List<Entity>
                        {
                            new Entity { EntityName = "Date", EntityValue = dateEntity },
                        };
                        //Update conversation with date entity
                        await conversationService.UpdateConversationAsync(sessionId, entities: entities);
                        var con = await conversationService.GetConversationAsync(sessionId);
                        Request.Cookies.TryGetValue("Token", out var token);
                        if (token == null)
                        {
                            //Update conversation to get email
                            await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Email");
                            return "Please enter your email 📧";
                        }
                        return await HandleIntent(sessionId: sessionId, intent: con.Intent, entities: con.Entities.ToList(), token: token);
                    }
                    return "Please enter the date 📅";

                case "Time":
                    var timeEntityJson = resp.RootElement.GetProperty("entities")
                                                        .GetProperty("time");
                    if (timeEntityJson.ValueKind != JsonValueKind.Null)
                    {
                        var timeEntity = timeEntityJson.GetString();
                        //Add Time Entity to Database
                        var entities = new List<Entity>
                        {
                            new Entity { EntityName = "Time", EntityValue = timeEntity }
                        };
                        //Update conversation with time entity
                        await conversationService.UpdateConversationAsync(sessionId, entities: entities);
                        var con = await conversationService.GetConversationAsync(sessionId);
                        Request.Cookies.TryGetValue("Token", out var token);
                        if (token == null)
                        {
                            //Update conversation to get email
                            await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Email");
                            return "Please enter your email 📧";
                        }
                        return await HandleIntent(sessionId: sessionId, intent: con.Intent, entities: entities, token: token);
                    }
                    return "Please enter the time ⏰";
                default:
                    return "Please type your query below ✍️";
            }
        }

        //User When login
        private async Task<string> HandleLogin(int sessionId, string email)
        {
            var entities = new List<Entity>
            {
                new Entity { EntityName = "Email", EntityValue = email}
            };

            //Added the email entity to database
            await conversationService.UpdateConversationAsync(sessionId: sessionId, entities: entities);

            //check if the user is present in the database else add to database
            await loginService.GetUser(email);

            //send Otp via email
            var otp = otpService.GenerateOtp();
            await otpService.SendOtpViaMail(to: email, subject: "Your OTP Code", body: $"Thank you for using our service. Your one-time password (OTP) to access your account is:\r\n\r\n{otp}\r\n\r\n Please note that this OTP is valid for a limited time (e.g., 5 minutes). Do not share this code with anyone. If you did not request this OTP, please ignore this message.\r\n");
            await otpService.StoreOtp(email: email, otp: otp);

            //Update conversation to get otp
            await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Otp");
            //await conversationService.DeleteEntitiesAsync(sessionId: sessionId);
            return "OTP sent to your registered email. Please enter it below. 📩";
        }

        //Handle the intents of User Query
        private async Task<string> HandleIntent(int sessionId, string intent, List<Entity>? entities, string token)
        {
            //Extract the UserId from token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userId = int.Parse(userIdClaim ?? throw new ArgumentNullException(nameof(userIdClaim)));

            //User query intents
            switch (intent)
            {
                //View Appointment Query
                case "Appointment":
                    var appointments = await appointmentService.GetAppointments(userId);
                    string appointment = string.Join("\n\n", appointments.Select(a =>
                        $"**Date**: {a.AppointmentDate} \n **Time**: {a.AppointmentTime}"));

                    //Update the conversation to end (Delete also if required)
                    await conversationService.UpdateConversationAsync(sessionId: sessionId, IsCompleted: true, status: "End");
                    return "Your scheduled appointments: \n\n" + appointment;

                case "Schedule":
                    var conversation = await conversationService.GetConversationAsync(sessionId);
                    var dateEntity = conversation.Entities?.FirstOrDefault(e => e.EntityName == "Date");
                    var timeEntity = conversation.Entities?.FirstOrDefault(e => e.EntityName == "Time");

                    date = dateEntity?.EntityValue ?? "null";
                    time = timeEntity?.EntityValue ?? "null";
                    if (date == "null" || date == string.Empty)
                    {
                        //Update conversation to get date
                        await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Date");
                        return "Please enter the date 📅";
                    }
                    if (time == "null" || time == string.Empty)
                    {
                        //Update conversation to get time
                        await conversationService.UpdateConversationAsync(sessionId: sessionId, status: "Time");
                        return "Please enter the time ⏰";
                    }
                    
                    //Add Appointment to Database
                    var appointmentDto = new AppointmentDto
                    {
                        AppointmentDate = date,
                        AppointmentTime = time
                    };
                    var schedules = await appointmentService.AddAppointment(userId: userId, appointmentDto: appointmentDto);

                    //Update the conversation to end (Delete also if required)
                    await conversationService.UpdateConversationAsync(sessionId: sessionId, IsCompleted: true, status: "End");
                    await conversationService.DeleteEntitiesAsync(sessionId: sessionId);
                    return schedules;

                //View Prescription Query
                case "Prescription":
                    var prescriptions = await prescriptionService.GetPrescriptions(userId);
                    string prescription = string.Join("\n\n", prescriptions.Select(p =>
                        $"**Medicine Name**: {p.MedicineName} \n **Medicine Dosage**: {p.MedicineDosage} \n **Medicine Direction**: {p.MedicineDirection}"));

                    //Update the conversation to end (Delete also if required)
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "End");
                    return "Your prescriptions: \n\n" + prescription;

                //View Insurance Query
                case "Insurance":
                    var insuranceDetails = await insuranceService.GetInsuranceDetails(userId);
                    string insurance = string.Join("\n\n", insuranceDetails.Select(i =>
                        $"**Insurance Name **: {i.InsuranceName} \n **Start Date**: {i.InsuranceStart} \n **End Date**: {i.InsuranceEnd}, \n **Status**: {i.InsuranceStatus}"));

                    //Update the conversation to end (Delete also if required)
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "End");
                    return "Your Insurance: \n\n" + insurance;

                //View Payments Query
                case "Payment":
                    var payments = await paymentService.GetDuePayments(userId);
                    string payment = string.Join("\n\n", payments.Select(p =>
                        $"**Payment Due**: {p.PaymentDue} \n **Amount**: Rs {p.PaymentAmount} \n **Status**: {p.PaymentStatus}"));

                    //Update the conversation to end (Delete also if required)
                    await conversationService.UpdateConversationAsync(sessionId, IsCompleted: true, status: "End");
                    return "Your Payments: \n\n" + payment;

                default:
                    return "Couldn't process your request. Please try again. 🔄";
            }
        }
    }
}
