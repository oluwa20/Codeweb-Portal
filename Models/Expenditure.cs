using System.ComponentModel.DataAnnotations;

namespace SMS.Models
{
    public class Expenditure
    {
        [Key]
        public int Id { get; set; }
        public string? Expenses { get; set; }
        public  decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime? Date { get; set; }
    }
}
