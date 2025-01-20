using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface ILoginService
    {
        Task<Int32> CheckUser(string email);
    }
}
