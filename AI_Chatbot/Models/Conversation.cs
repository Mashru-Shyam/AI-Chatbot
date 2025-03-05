using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int SessionId { get; set; }
        public string Intent { get; set; } = "None"; //None, Prescription, Appointment, Schedule, Payment, Insurance
        public ICollection<Entity> Entities { get; set; } = new List<Entity>();
        public bool IsCompleted { get; set; } //0, 1
        public string Context { get; set; } = "Start"; //Start, Otp, Date, Time, Email, end

    }
}
