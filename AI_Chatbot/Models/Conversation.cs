using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int UserId { get; set; }
        public string Intent { get; set; }
        public ICollection<Entity> Entities { get; set; }
        public bool IsCompleted { get; set; }
        public string Context { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }

    }
}
