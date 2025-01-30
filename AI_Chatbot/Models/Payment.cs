using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public DateTime PaymentDue { get; set; } 
        public decimal PaymentAmount { get; set; }
        public string PaymentStatus { get; set; } = "pending"; //pending, progress, completed
        
        [ForeignKey("UserId")]
        public User? User { get; set; }  
    }
}
