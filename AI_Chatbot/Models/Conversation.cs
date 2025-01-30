using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int SessionId { get; set; }
        public string Intent { get; set; } = "none"; //none, appointments, schedule, prescriptions, payments, insurance, login
        public ICollection<Entity> Entities { get; set; } = new List<Entity>();
        public bool IsCompleted { get; set; } //true, false
        public string Context { get; set; } = "start"; //start, otp, date, time, email, end

    }
}
