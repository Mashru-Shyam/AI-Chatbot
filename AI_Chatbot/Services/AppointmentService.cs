using AI_Chatbot.Datas;
using AI_Chatbot.DTOs;
using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Services
{
    public class AppointmentService : IAppointmentService
    {
        private AiChatbotDbContext context;

        public AppointmentService(AiChatbotDbContext context)
        {
            this.context = context;
        }

        public async Task<string> AddAppointment(int userId, AppointmentDto appointmentDto)
        {
            var appointment = new Appointment
            {
                UserId = userId,
                AppointmentDate = appointmentDto.AppointmentDate,
                AppointmentTime = appointmentDto.AppointmentTime,
            };

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            return $"Appointment added successfully at {appointmentDto.AppointmentDate} and {appointmentDto.AppointmentTime}";
        }

        public async Task<IEnumerable<AppointmentDto>> GetAppointments(int userId)
        {
            var appointments = await context.Appointments
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var result = appointments.Select(a => new AppointmentDto
            {
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime
            }).ToList();

            return result;
        }
    }
}
