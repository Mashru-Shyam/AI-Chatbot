using System.ComponentModel.DataAnnotations;

namespace AI_Chatbot.DTOs
{
    public class AppointmentDto
    {
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly AppointmentTime { get; set; }
    }
}
