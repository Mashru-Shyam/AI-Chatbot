using AI_Chatbot.DTOs;
using AI_Chatbot.Models;

namespace AI_Chatbot.Interfaces
{
    public interface IConversationService
    {
        Task<Conversation> GetConversationAsync(int sessionId);
        Task AddConversationAsync(int sessionId, string intent, ICollection<Entity>? entities, bool IsCompleted, string status);
        Task UpdateConversationAsync(int sessionId, string intent = "None", ICollection<Entity>? entities = null, bool IsCompleted = false, string status = "Start");
        Task DeleteConversationAsync(int sessionId);
        Task DeleteEntitiesAsync(int sessionId);
    }
}
