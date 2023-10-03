using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace SMS.Models
{
    public class Student
    {
        [Key]
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public List<Payment> Payments { get; set; } = new List<Payment>();
    }
}
