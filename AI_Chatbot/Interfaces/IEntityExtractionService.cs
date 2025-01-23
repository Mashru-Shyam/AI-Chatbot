using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IEntityExtractionService
    {
        Dictionary<string, List<string>> ExtractEntities(string query);
    }
}
