using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public string? AppointmentDate { get; set; } //Valid Date
        public string AppointmentTime { get; set; } = string.Empty; //Valid Time

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
