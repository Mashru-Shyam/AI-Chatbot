using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public string? AppointmentDate { get; set; } //Format dd/mm/yy
        public string AppointmentTime { get; set; } = string.Empty; //Format HH:mm AM/PM

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
