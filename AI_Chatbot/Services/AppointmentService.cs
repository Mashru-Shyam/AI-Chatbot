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

        //Add Appointments To Dataset
        public async Task<string> AddAppointment(int userId, AppointmentDto appointmentDto)
        {
            var appointment = new Appointment
            {
                UserId = userId,
                AppointmentDate = appointmentDto.AppointmentDate,
                AppointmentTime = appointmentDto.AppointmentTime ?? string.Empty,
            };

            await context.Appointments.AddAsync(appointment);
            await context.SaveChangesAsync();

            return $"Appointment added successfully at \n\n**Date**: {appointmentDto.AppointmentDate}\n**Time**: {appointmentDto.AppointmentTime}";
        }

        //Retrive Appointment Details
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
