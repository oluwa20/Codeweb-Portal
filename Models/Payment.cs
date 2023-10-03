namespace SMS.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? Date { get; set; }


        public Guid StudentId { get; set; }
        public Student Student { get; set; }
    }
}
