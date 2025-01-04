using AI_Chatbot.DTOs;
using AI_Chatbot.Models;

namespace AI_Chatbot.Interfaces
{
    public interface IAppointmentService
    {
        Task<string> AddAppointment(int userId, AppointmentDto appointmentDto);
        Task<IEnumerable<AppointmentDto>> GetAppointments(int userId);
    }
}
