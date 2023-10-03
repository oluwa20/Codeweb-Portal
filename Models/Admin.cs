using System.ComponentModel.DataAnnotations;

namespace SMS.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        public  string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
