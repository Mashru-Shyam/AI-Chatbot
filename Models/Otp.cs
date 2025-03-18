using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Otp
    {
        [Key]
        public int OtpId { get; set; }
        public int UserId { get; set; }
        public string? OtpEmail { get; set; }
        public string? OtpCode { get; set; }
        public DateTime OtpExpirationTime { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
