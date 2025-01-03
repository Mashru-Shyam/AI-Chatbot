namespace AI_Chatbot.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp(int length=6);
        Task SendOtpViaMail(string to, string subject, string body);
        Task StoreOtp(string email, string code);
        Task<string> CheckOtp(string code);
    }
}
