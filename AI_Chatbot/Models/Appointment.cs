namespace AI_Chatbot.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int UserId { get; set; }
        public DateOnly AppointmentDate { get; set; }
        public TimeOnly AppointmentTime { get; set; }
    }
}
