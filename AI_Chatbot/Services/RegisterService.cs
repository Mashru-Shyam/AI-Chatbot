using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly AiChatbotDbContext context;

        public RegisterService(AiChatbotDbContext context)
        {
            this.context = context;
        }
        public async Task Register(LoginDto login)
        {
            //var existUser = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == login.Email);
            //if (existUser != null)
            //{
            //    return "User already exists...";
            //}
            var user = new User
            {
                UserEmail = login.Email,
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }
    }
}
