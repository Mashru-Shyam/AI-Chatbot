using AI_Chatbot.Datas;
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
        public async Task<string> Register(string email)
        {
            var existUser = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
            if (existUser != null)
            {
                return "User already exists...";
            }
            var user = new User
            {
                UserEmail = email
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return "Registration Successful...";
        }
    }
}
