using System;

namespace Project_Recruiment_Huce.Models.Payment
{
    /// <summary>
    /// Webhook model received from SePay
    /// </summary>
    public class SePayWebhookModel
    {
        public int id { get; set; }
        public string gateway { get; set; }
        public string transactionDate { get; set; } // SePay sends as string
        public string accountNumber { get; set; }
        public string subAccount { get; set; }
        public decimal amountIn { get; set; }
        public decimal amountOut { get; set; }
        public decimal accumulated { get; set; }
        public string code { get; set; }
        public string transactionContent { get; set; }
        public string referenceCode { get; set; }
        public string description { get; set; }
    }

    /// <summary>
    /// Result of webhook processing
    /// </summary>
    public class WebhookProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsDuplicate { get; set; }
        public int? RecruiterID { get; set; }
        public string PlanId { get; set; }
    }

    /// <summary>
    /// Upgrade request DTO
    /// </summary>
    public class SubscriptionUpgradeRequest
    {
        public int RecruiterID { get; set; }
        public string PlanId { get; set; }
        public decimal AmountPaid { get; set; }
        public string ReferenceCode { get; set; }
    }

    /// <summary>
    /// Subscription status DTO
    /// </summary>
    public class SubscriptionStatusDto
    {
        public string CurrentPlan { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int FreeJobPostCount { get; set; }
        public bool IsUpgraded { get; set; }
    }
}
