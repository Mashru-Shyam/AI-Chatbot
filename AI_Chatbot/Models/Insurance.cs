using System.ComponentModel.DataAnnotations;

namespace AI_Chatbot.Models
{
    public class Insurance
    {
        [Key]
        public int InsuranceId { get; set; }
        public int UserId { get; set; }
        public string InsuranceName { get; set; }
        public DateTime InsuranceStart { get; set; }
        public DateTime InsuranceEnd { get; set; }
        public string InsuranceStatus { get; set; }
    }
}
