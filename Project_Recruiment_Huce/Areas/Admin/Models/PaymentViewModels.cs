using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách giao dịch
    /// </summary>
    public class TransactionListVm
    {
        public int TransactionId { get; set; }
        public string TransactionNo { get; set; }
        public string AccountEmail { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public string Status { get; set; }
        public DateTime TransactedAt { get; set; }
    }

    /// <summary>
    /// ViewModel cho thẻ ngân hàng
    /// </summary>
    public class BankCardListVm
    {
        public int CardId { get; set; }
        public string CompanyName { get; set; }
        public string MaskedNumber { get; set; }
        public string BankName { get; set; }
        public bool Active { get; set; }
    }

    /// <summary>
    /// ViewModel cho thanh toán chờ xử lý
    /// </summary>
    public class PendingPaymentVm
    {
        public int PendingId { get; set; }
        public string CompanyName { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// ViewModel cho lịch sử thanh toán
    /// </summary>
    public class PaymentHistoryVm
    {
        public int PaymentId { get; set; }
        public string CompanyName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
    }
}

