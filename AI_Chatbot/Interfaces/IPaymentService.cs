using AI_Chatbot.DTOs;
using AI_Chatbot.Models;

namespace AI_Chatbot.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentDto>> GetDuePayments(int userId);
    }
}
