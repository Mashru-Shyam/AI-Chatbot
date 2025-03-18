namespace AI_Chatbot.DTOs
{
    public class PaymentDto
    {
        public string? PaymentDue { get; set; } //Format dd/mm/yy
        public decimal PaymentAmount { get; set; }
        public string? PaymentStatus { get; set; } //Pending, Paid, Overdue
    }
}
