using System.ComponentModel.DataAnnotations;

namespace AI_Chatbot.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string? UserEmail { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Prescription>? Prescriptions { get; set; }
        public ICollection<Insurance>? Insurances { get; set; }
        public ICollection<Otp>? Otps { get; set; }
    }
}
