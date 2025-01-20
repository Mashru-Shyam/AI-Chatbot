using AI_Chatbot.DTOs;
using AI_Chatbot.Models;

namespace AI_Chatbot.Interfaces
{
    public interface IConversationService
    {
        Task<Conversation> GetConversationAsync(int userId);
        Task AddConversationAsync(int userId, string intent, ICollection<Entity> entities, bool IsCompleted, string status);
        Task UpdateConversationAsync(int userId, string intent, ICollection<Entity> entities, bool IsCompleted, string status);
        Task DeleteConversationAsync(int userId);
    }
}
