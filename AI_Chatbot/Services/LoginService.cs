using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class LoginService : ILoginService
    {
        private readonly AiChatbotDbContext context;

        public LoginService(AiChatbotDbContext context)
        {
            this.context = context;
        }
        //Retrive the UserId
        public async Task<Int32> GetUser(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
            if (user == null)
            {
                return 0;
            }
            return user.UserId;
        }

        //Register a User
        public async Task AddUser(string email)
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
