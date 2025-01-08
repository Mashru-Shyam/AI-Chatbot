using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface ILoginService
    {
        Task<bool> CheckUser(LoginDto login);
    }
}
