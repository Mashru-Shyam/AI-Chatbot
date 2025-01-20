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
        public async Task<Int32> CheckUser(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserEmail == email);
            if (user == null)
            {
                return 0;
            }
            return user.UserId;
        }
    }
}
