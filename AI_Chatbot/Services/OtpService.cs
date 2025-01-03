using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace AI_Chatbot.Services
{
    public class OtpService : IOtpService
    {
        private static Random _random = new Random();
        private readonly AiChatbotDbContext context;
        private readonly IJwtService jwtService;
        private readonly SmtpClient _smtpClient;
        private readonly string _username;
        public OtpService(AiChatbotDbContext context, IOptions<EmailSettings> options, IJwtService jwtService)
        {
            this.context = context;
            this.jwtService = jwtService;
            var settings = options.Value;
            _username = settings.Username;

            _smtpClient = new SmtpClient(settings.Host, settings.Port)
            {
                Credentials = new NetworkCredential(settings.Username, settings.Password),
                EnableSsl = true
            };
        }
        public string GenerateOtp(int length = 6)
        {
            const string chars = "0123456789";
            char[] otp = new char[length];
            for (int i = 0; i < length; i++)
            {
                otp[i] = chars[_random.Next(chars.Length)];
            }
            return new string(otp);
        }

        public async Task SendOtpViaMail(string to, string subject, string body)
        {
            var mail = new MailMessage(_username, to)
            {
                Subject = subject,
                Body = body
            };
            await _smtpClient.SendMailAsync(mail);
        }

        public async Task StoreOtp(string email, string code)
        {
            var userId = await context.Users
                .Where(u => u.UserEmail == email)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            var otpEntity = new Otp
            {
                UserId = userId,
                OtpEmail = email,
                OtpCode = code,
                OtpExpirationTime = DateTime.UtcNow.AddMinutes(5)
            };

            await context.Otps.AddAsync(otpEntity);
            await context.SaveChangesAsync();
        }

        public async Task<string> CheckOtp(string code)
        {
            var otp = await context.Otps
                .Where(o=>o.OtpCode == code && o.OtpExpirationTime > DateTime.UtcNow)
                .OrderByDescending(o => o.OtpExpirationTime)
                .FirstOrDefaultAsync();

            if(otp == null)
            {
                return null;
            }

            return jwtService.GetToken(otp.UserId);
        }
    }
}
