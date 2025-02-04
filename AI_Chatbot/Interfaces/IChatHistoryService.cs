using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IChatHistoryService
    {
        public Task AddChatHistory(int sessionId, string userMessage, string botResponse);
        public Task<IEnumerable<ChatHistoryDto>> GetChatHistory(int sessionId);
    }
}
