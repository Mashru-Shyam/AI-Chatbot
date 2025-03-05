using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface ILoginService
    {
        Task GetUser(string email);
        Task AddUser(string email);
    }
}
