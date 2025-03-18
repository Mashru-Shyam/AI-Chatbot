namespace AI_Chatbot.Interfaces
{
    public interface IQueryService
    {
        Task<string> IntentClassification(int sessionId, string query);
        Task<string> EntityExtraction(int sessionId, string query);
    }
}
