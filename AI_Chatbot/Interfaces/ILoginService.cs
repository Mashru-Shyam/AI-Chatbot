using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface ILoginService
    {
        Task<bool> GetUser(string email);
        Task AddUser(string email);
    }
}
