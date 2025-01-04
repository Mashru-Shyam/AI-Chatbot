using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class InsuranceService : IInsuranceService
    {
        private readonly AiChatbotDbContext context;

        public InsuranceService(AiChatbotDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<InsuranceDto>> GetInsuranceDetails(int userId)
        {
            var insurance = await context.Insurances
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var result = insurance.Select(p => new InsuranceDto
            {
                InsuranceName = p.InsuranceName,
                InsuranceStart = p.InsuranceStart,
                InsuranceEnd = p.InsuranceEnd,
                InsuranceStatus = p.InsuranceStatus
            }).ToList();

            return result;
        }
    }
}
