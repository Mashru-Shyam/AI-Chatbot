namespace AI_Chatbot.Models
{
    public class Entity
    {
        public int EntityId { get; set; }
        public int ConversationId { get; set; }
        public string EntityName { get; set; }
        public string EntityValue { get; set; }
        public Conversation Conversation { get; set; }

    }
}
