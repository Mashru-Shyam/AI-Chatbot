using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IOtpService
    {
        string GenerateOtp(int length=6);
        Task SendOtpViaMail(string to, string subject, string body);
        Task StoreOtp(LoginDto login, string otp);
        Task<string> CheckOtp(OtpDto otp);
    }
}
