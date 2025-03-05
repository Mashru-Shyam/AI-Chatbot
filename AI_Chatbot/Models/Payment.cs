using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public string? PaymentDue { get; set; }  //Format : dd/mm/yy
        public decimal PaymentAmount { get; set; }
        public string? PaymentStatus { get; set; } //Pending, Paid, Overdue
        
        [ForeignKey("UserId")]
        public User? User { get; set; }  
    }
}
