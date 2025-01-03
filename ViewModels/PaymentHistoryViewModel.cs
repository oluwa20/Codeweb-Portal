﻿using SMS.Models;

namespace SMS.ViewModels
{
    public class PaymentHistoryViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public List<Payment> Payments { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? Date { get; set; }
        public Payment Payment { get; internal set; }
    }

    public class EditPaymentViewModel
    {
        public Guid PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime? Date { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
    }

}
