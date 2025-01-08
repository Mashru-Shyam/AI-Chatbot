using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;

namespace AI_Chatbot.Services
{
    public class OtpService : IOtpService
    {
        private static Random _random = new Random();
        private readonly AiChatbotDbContext context;
        private readonly IOptions<EmailSettings> options;
        private readonly IJwtService jwtService;
        private readonly ISmtpClient smtpClient;
        private readonly string _username;
        public OtpService(AiChatbotDbContext context, IOptions<EmailSettings> options, IJwtService jwtService, ISmtpClient smtpClient)
        {
            this.context = context;
            this.options = options;
            this.jwtService = jwtService;
            this.smtpClient = smtpClient;
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
            var mail = new MimeMessage();
            mail.From.Add(new MailboxAddress("AI Chatbot", options.Value.Username));
            mail.To.Add(new MailboxAddress("", to));
            mail.Subject = subject;
            mail.Body = new TextPart("plain")
            {
                Text = body
            };

            smtpClient.Connect(options.Value.Host, options.Value.Port, MailKit.Security.SecureSocketOptions.StartTls);
            smtpClient.Authenticate(options.Value.Username, options.Value.Password);
            await smtpClient.SendAsync(mail);
            smtpClient.Disconnect(true);
        }

        public async Task StoreOtp(LoginDto login, string otp)
        {
            var userId = await context.Users
                .Where(u => u.UserEmail == login.Email)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            var otpEntity = new Otp
            {
                UserId = userId,
                OtpEmail = login.Email,
                OtpCode = otp,
                OtpExpirationTime = DateTime.UtcNow.AddMinutes(5)
            };

            await context.Otps.AddAsync(otpEntity);
            await context.SaveChangesAsync();
        }

        public async Task<string> CheckOtp(OtpDto otp)
        {
            var validotp = await context.Otps
                .Where(o=>o.OtpCode == otp.Code && o.OtpExpirationTime > DateTime.UtcNow)
                .OrderByDescending(o => o.OtpExpirationTime)
                .FirstOrDefaultAsync();

            if(validotp == null)
            {
                return null;
            }

            return jwtService.GetToken(validotp.UserId);
        }
    }
}
