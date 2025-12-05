using System;

namespace Project_Recruiment_Huce.Models
{
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
}
