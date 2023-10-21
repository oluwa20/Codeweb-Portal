using SMS.Models;

namespace SMS.ViewModels
{
    public class MonthlyPaymentsViewModel
    {
        public List<Payment> Payments { get; set; }
        public decimal TotalAmount { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
