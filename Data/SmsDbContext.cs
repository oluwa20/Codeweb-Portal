using Microsoft.EntityFrameworkCore;
using SMS.Models;
namespace SMS.Data
{
    public class SmsDbContext: DbContext
    {
        public SmsDbContext(DbContextOptions<SmsDbContext> options) : base(options)
        {
            
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}
