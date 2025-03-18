using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly AiChatbotDbContext context;

        public PrescriptionService(AiChatbotDbContext context)
        {
            this.context = context;
        }

        //Retrive Prescription
        public async Task<IEnumerable<PrescriptionDto>> GetPrescriptions(int userId)
        {
            var prescription = await context.Prescriptions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var result = prescription.Select(p => new PrescriptionDto
            {
                MedicineDirection = p.MedicineDirection,
                MedicineName = p.MedicineName,
                MedicineDosage = p.MedicineDosage,

            }).ToList();

            return result;
        }
    }
}
