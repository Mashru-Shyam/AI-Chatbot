namespace AI_Chatbot.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int UserId { get; set; }
        public DateTime PaymentDue { get; set; }
        public double PaymentAmount { get; set; }
        public string PaymentStatus { get; set; }
    }
}
