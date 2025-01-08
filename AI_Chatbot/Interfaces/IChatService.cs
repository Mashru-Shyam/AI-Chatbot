using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IChatService
    {
        Task<string> Chatting (ChatRequestDto chatRequest);
    }
}
