using AI_Chatbot.DTOs;

namespace AI_Chatbot.Interfaces
{
    public interface IInsuranceService
    {
        Task<IEnumerable<InsuranceDto>> GetInsuranceDetails(int userId);
    }
}
