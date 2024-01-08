using System.ComponentModel.DataAnnotations;

namespace SMS.ViewModels
{
    public class PaymentViewModel
    {
        public Guid StudentId { get; set; }
        /*public string? StudentName { get; set; }*/

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }
        public Guid TransactionId { get; set; }
        public decimal AmountPaid { get; set; }
        public string MonthPaid { get; set; }
        public string StudentName { get; set; }
        public string ReceiptDateTime { get; set; }
    }
}
