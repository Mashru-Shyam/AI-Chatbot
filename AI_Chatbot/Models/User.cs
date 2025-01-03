using System.ComponentModel.DataAnnotations;

namespace AI_Chatbot.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string UserEmail { get; set; }
    }
}
