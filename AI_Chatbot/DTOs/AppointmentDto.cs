using System.ComponentModel.DataAnnotations;

namespace AI_Chatbot.DTOs
{
    public class AppointmentDto
    {
        public string? AppointmentDate { get; set; }
        public string? AppointmentTime { get; set; }
    }
}
