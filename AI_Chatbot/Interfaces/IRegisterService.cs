namespace AI_Chatbot.Interfaces
{
    public interface IRegisterService
    {
        Task<string> Register(string email);
    }
}
