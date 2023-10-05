using SMS.Models;

namespace SMS.ViewModels
{
    public class PaymentHistoryViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public List<Payment> Payments { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? Date { get; set; }
    }
}
