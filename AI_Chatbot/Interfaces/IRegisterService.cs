using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IRegisterService
    {
        Task Register(LoginDto login);
    }
}
