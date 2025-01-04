namespace AI_Chatbot.DTOs
{
    public class PaymentDto
    {
        public DateTime PaymentDue { get; set; }
        public double PaymentAmount { get; set; }
        public string PaymentStatus { get; set; }
    }
}
