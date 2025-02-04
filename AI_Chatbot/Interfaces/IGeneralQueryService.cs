using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IGeneralQueryService
    {
        Task<string> GeneralQuery(int sessionId, string query);
    }
}
