using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int SessionId { get; set; }
        public string? Intent { get; set; }
        public ICollection<Entity>? Entities { get; set; }
        public bool IsCompleted { get; set; } //true, false
        public string? Context { get; set; } // Started, InProgress, Completed

    }
}
