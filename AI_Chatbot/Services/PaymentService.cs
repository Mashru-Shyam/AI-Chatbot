using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AiChatbotDbContext context;

        public PaymentService(AiChatbotDbContext context)
        {
            this.context = context;
        }

        //Retrive Due Payments
        public async Task<IEnumerable<PaymentDto>> GetDuePayments(int userId)
        {
            var payment = await context.Payment
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var result = payment.Select(p => new PaymentDto
            {
                PaymentAmount = p.PaymentAmount,
                PaymentDue = p.PaymentDue,
                PaymentStatus = p.PaymentStatus
            }).ToList();

            return result;
        }
    }
}
