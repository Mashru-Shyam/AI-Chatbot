namespace AI_Chatbot.Interfaces
{
    public interface IChatService
    {
        Task<string> Chatting (string query);
    }
}
