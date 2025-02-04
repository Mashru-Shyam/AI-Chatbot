using System.ComponentModel.DataAnnotations;

namespace AI_Chatbot.Models
{
    public class ChatHistory
    {
        [Key]
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string? UserMessage { get; set; }
        public string? BotMessage { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
