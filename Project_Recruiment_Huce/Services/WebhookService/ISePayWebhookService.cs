using System;
using SePayModel = Project_Recruiment_Huce.Models.Payment.SePayWebhookModel;
using Project_Recruiment_Huce.Models.Payment;

namespace Project_Recruiment_Huce.Services.WebhookService
{
    /// <summary>
    /// Service interface for SePay webhook processing
    /// </summary>
    public interface ISePayWebhookService
    {
        /// <summary>
        /// Validate webhook security (API Key, IP, Signature, Timestamp)
        /// </summary>
        /// <param name="apiKey">API Key from request header</param>
        /// <param name="ipAddress">Client IP address</param>
        /// <param name="signature">Optional signature for verification</param>
        /// <param name="payload">Webhook payload for signature validation</param>
        /// <param name="timestamp">Transaction timestamp</param>
        /// <returns>Validation result</returns>
        (bool isValid, string errorMessage) ValidateWebhookSecurity(
            string apiKey, 
            string ipAddress, 
            string signature, 
            string payload, 
            DateTime? timestamp);

        /// <summary>
        /// Process webhook and handle subscription upgrade
        /// </summary>
        /// <param name="model">Webhook data from SePay</param>
        /// <returns>Processing result</returns>
        WebhookProcessingResult ProcessWebhook(SePayModel model);

        /// <summary>
        /// Check if transaction is duplicate (idempotency)
        /// </summary>
        bool IsDuplicateTransaction(string referenceCode);
    }
}
