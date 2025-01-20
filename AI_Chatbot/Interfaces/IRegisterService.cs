using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IRegisterService
    {
        Task Register(string email);
    }
}
