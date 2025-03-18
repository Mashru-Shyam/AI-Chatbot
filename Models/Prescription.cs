using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AI_Chatbot.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }
        public int UserId { get; set; }
        public string? MedicineName { get; set; }
        public string? MedicineDosage { get; set; }
        public string? MedicineDirection { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
