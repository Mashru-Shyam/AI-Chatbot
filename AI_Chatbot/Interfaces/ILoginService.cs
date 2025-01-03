namespace AI_Chatbot.Interfaces
{
    public interface ILoginService
    {
        Task<bool> CheckUser(string email);
    }
}
