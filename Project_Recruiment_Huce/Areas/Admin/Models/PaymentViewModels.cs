using System;

namespace Project_Recruiment_Huce.Areas.Admin.Models
{
    /// <summary>
    /// ViewModel cho danh sách giao dịch (Transaction table)
    /// </summary>
    public class TransactionListVm
    {
        public int TransactionId { get; set; }
        public int? AccountId { get; set; }
        public decimal? Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime? TransactedAt { get; set; }
        public string Description { get; set; }
        
        // Helper properties for display (from related Account table)
        public string AccountEmail { get; set; }
        public string TransactionNo { get; set; } // Generated or from external system
    }

    /// <summary>
    /// ViewModel cho thẻ ngân hàng
    /// </summary>
    public class BankCardListVm
    {
        public int CardId { get; set; }
        public int CompanyId { get; set; }
        public string CardNumber { get; set; } // Full number (will be masked in display)
        public string BankName { get; set; }
        public string CardHolderName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public byte? ActiveFlag { get; set; } // 1 = active, 0 = inactive
        
        // Helper properties for display (from related Company table)
        public string CompanyName { get; set; }
        
        // Helper property for display
        public bool Active => ActiveFlag == 1;
        public string MaskedNumber => AdminUiHelpers.Mask(CardNumber ?? string.Empty);
    }

    /// <summary>
    /// ViewModel cho thanh toán chờ xử lý
    /// </summary>
    public class PendingPaymentVm
    {
        public int PendingId { get; set; }
        public int CompanyId { get; set; }
        public decimal AmountDue { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Helper property for display (from related Company table)
        public string CompanyName { get; set; }
    }

    /// <summary>
    /// ViewModel cho lịch sử thanh toán
    /// </summary>
    public class PaymentHistoryVm
    {
        public int PaymentId { get; set; }
        public int CompanyId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        
        // Helper property for display (from related Company table)
        public string CompanyName { get; set; }
    }
}

