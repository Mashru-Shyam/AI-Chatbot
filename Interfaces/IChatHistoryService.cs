using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IChatHistoryService
    {
        Task AddChatHistory(int sessionId, string userMessage, string botResponse);
        Task<IEnumerable<ChatHistoryDto>> GetChatHistory(int sessionId);
    }
}
