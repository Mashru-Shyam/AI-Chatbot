using AI_Chatbot.Models;
using Microsoft.EntityFrameworkCore;

namespace AI_Chatbot.Datas
{
    public class AiChatbotDbContext : DbContext
    {
        public AiChatbotDbContext(DbContextOptions<AiChatbotDbContext> options) : base(options)
        { }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
    }
}
