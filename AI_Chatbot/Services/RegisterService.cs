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
        public async Task Register(string email)
        {
            var user = new User
            {
                UserEmail = email,
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }
    }
}
