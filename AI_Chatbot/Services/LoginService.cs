using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
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
        public async Task<bool> CheckUser(LoginDto login)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == login.Email);
            if (user == null)
            {
                return false;
            }
            return true;
        }
    }
}
