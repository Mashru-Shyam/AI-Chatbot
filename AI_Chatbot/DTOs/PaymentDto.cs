namespace AI_Chatbot.DTOs
{
    public class PaymentDto
    {
        public DateTime PaymentDue { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentStatus { get; set; }
    }
}
