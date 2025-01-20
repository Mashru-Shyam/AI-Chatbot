using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IIntentClassificationService
    {
        Task<string> Chatting (ChatRequestDto chatRequest);
    }
}
