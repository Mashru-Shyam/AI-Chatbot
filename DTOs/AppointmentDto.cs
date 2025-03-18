using System.ComponentModel.DataAnnotations;

namespace AI_Chatbot.DTOs
{
    public class AppointmentDto
    {
        public string? AppointmentDate { get; set; } //Format dd/mm/yy
        public string? AppointmentTime { get; set; } //Format HH:mm AM/PM
    }
}
