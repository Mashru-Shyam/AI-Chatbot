using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IPrescriptionService
    {
        Task<IEnumerable<PrescriptionDto>> GetPrescriptions(int userId);
    }
}
