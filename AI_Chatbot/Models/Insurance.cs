using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Insurance
    {
        [Key]
        public int InsuranceId { get; set; }
        public int UserId { get; set; }
        public string? InsuranceName { get; set; }
        public string? InsuranceStart { get; set; }
        public string? InsuranceEnd { get; set; }
        public string InsuranceStatus { get; set; } = "pending"; //dective, active

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
