using AI_Chatbot.Models;

namespace AI_Chatbot.Interfaces
{
    public interface IJwtService
    {
        string GetToken(int userId);
    }
}
