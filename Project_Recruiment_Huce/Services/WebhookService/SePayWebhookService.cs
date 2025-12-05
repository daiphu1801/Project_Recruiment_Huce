using System;
using System.Configuration;
using System.Linq;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Payment;
using Project_Recruiment_Huce.Repositories.SubscriptionRepo;
using SubscriptionServiceNS = Project_Recruiment_Huce.Services.SubscriptionService;
using SePayModel = Project_Recruiment_Huce.Models.Payment.SePayWebhookModel;

namespace Project_Recruiment_Huce.Services.WebhookService
{
    /// <summary>
    /// Service for SePay webhook processing
    /// </summary>
    public class SePayWebhookService : ISePayWebhookService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly SubscriptionServiceNS.ISubscriptionService _subscriptionService;

        public SePayWebhookService() : this(new SubscriptionRepository(), new SubscriptionServiceNS.SubscriptionService())
        {
        }

        public SePayWebhookService(
            ISubscriptionRepository subscriptionRepository,
            SubscriptionServiceNS.ISubscriptionService subscriptionService)
        {
            _subscriptionRepository = subscriptionRepository ?? throw new ArgumentNullException(nameof(subscriptionRepository));
            _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        }

        /// <summary>
        /// Validate webhook security (API Key, IP, Signature, Timestamp)
        /// </summary>
        public (bool isValid, string errorMessage) ValidateWebhookSecurity(
            string apiKey,
            string ipAddress,
            string signature,
            string payload,
            DateTime? timestamp)
        {
            // Security Check 1: API Key Validation
            var expectedKey = ConfigurationManager.AppSettings["SePay:WebhookSecret"];

            if (string.IsNullOrEmpty(apiKey))
            {
                PaymentLogger.Warning($"Webhook rejected: Missing API Key from IP {ipAddress}");
                return (false, "Thiếu API Key");
            }

            // Remove common prefixes
            var prefixes = new[] { "Bearer ", "Apikey ", "ApiKey ", "API-Key " };
            foreach (var prefix in prefixes)
            {
                if (apiKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    apiKey = apiKey.Substring(prefix.Length).Trim();
                    break;
                }
            }

            if (apiKey != expectedKey)
            {
                PaymentLogger.Warning($"Webhook rejected: Invalid API Key from IP {ipAddress}");
                return (false, "API Key không hợp lệ");
            }

            PaymentLogger.Info($"API Key validated successfully from IP {ipAddress}");

            // Security Check 2: IP Whitelist Validation
            if (!SePaySecurityHelper.IsValidIP(ipAddress))
            {
                PaymentLogger.Warning($"Webhook rejected: Invalid IP {ipAddress}");
                return (false, "Không có quyền truy cập");
            }

            // Security Check 3: Signature Validation (if provided)
            if (!string.IsNullOrEmpty(signature))
            {
                if (!SePaySecurityHelper.ValidateSignature(payload, signature))
                {
                    PaymentLogger.Warning($"Webhook rejected: Invalid signature from IP {ipAddress}");
                    return (false, "Chữ ký không hợp lệ");
                }
            }

            // Security Check 4: Timestamp validation (prevent replay attacks)
            if (!SePaySecurityHelper.IsRequestTimestampValid(timestamp, maxAgeMinutes: 10))
            {
                PaymentLogger.Warning($"Webhook rejected: Request too old. Timestamp: {timestamp}");
                return (false, "Yêu cầu đã hết hạn");
            }

            return (true, null);
        }

        /// <summary>
        /// Check if transaction is duplicate (idempotency)
        /// </summary>
        public bool IsDuplicateTransaction(string referenceCode)
        {
            if (string.IsNullOrEmpty(referenceCode))
                return false;

            return _subscriptionRepository.TransactionExists(referenceCode);
        }

        /// <summary>
        /// Process webhook and handle subscription upgrade
        /// </summary>
        public WebhookProcessingResult ProcessWebhook(SePayModel model)
        {
            if (model == null)
            {
                PaymentLogger.Warning("ProcessWebhook: Model is null");
                return new WebhookProcessingResult
                {
                    Success = false,
                    Message = "Dữ liệu không hợp lệ"
                };
            }

            try
            {
                // Check for duplicate transaction (idempotency)
                if (IsDuplicateTransaction(model.referenceCode))
                {
                    PaymentLogger.Info($"Duplicate webhook detected. ReferenceCode: {model.referenceCode} already processed.");
                    return new WebhookProcessingResult
                    {
                        Success = true,
                        Message = "Đã xử lý trước đó",
                        IsDuplicate = true
                    };
                }

                PaymentLogger.Info($"Processing new webhook. ReferenceCode: {model.referenceCode}, Amount: {model.amountIn}");

                // Save transaction to database
                var transaction = new SePayTransaction
                {
                    Gateway = model.gateway,
                    TransactionDate = DateTime.TryParse(model.transactionDate, out var tDate) ? tDate : DateTime.Now,
                    AccountNumber = model.accountNumber,
                    SubAccount = model.subAccount,
                    AmountIn = model.amountIn,
                    AmountOut = model.amountOut,
                    Accumulated = model.accumulated,
                    Code = model.code,
                    TransactionContent = model.transactionContent,
                    ReferenceCode = model.referenceCode,
                    Description = model.description,
                    CreatedAt = DateTime.Now
                };
                _subscriptionRepository.SaveTransaction(transaction);

                PaymentLogger.Info($"Transaction saved. ID: {transaction.Id}, Amount: {transaction.AmountIn}");

                // Parse payment content
                string content = model.transactionContent ?? model.description;
                var (recruiterId, planId) = _subscriptionService.ParsePaymentContent(content);

                if (!recruiterId.HasValue || string.IsNullOrEmpty(planId))
                {
                    PaymentLogger.Info($"Transaction does not contain UPGRADE keyword. Content: {content}");
                    return new WebhookProcessingResult
                    {
                        Success = true,
                        Message = "Giao dịch không phải nâng cấp"
                    };
                }

                // Validate plan exists
                var plan = _subscriptionService.GetPlanById(planId);
                if (plan == null)
                {
                    PaymentLogger.Warning($"Plan not found: {planId}");
                    return new WebhookProcessingResult
                    {
                        Success = false,
                        Message = "Gói không tồn tại",
                        RecruiterID = recruiterId,
                        PlanId = planId
                    };
                }

                // Validate amount
                if (!_subscriptionService.ValidatePaymentAmount(planId, model.amountIn))
                {
                    PaymentLogger.Warning($"Amount mismatch. Required: {plan.Price}, Received: {model.amountIn}");
                    return new WebhookProcessingResult
                    {
                        Success = false,
                        Message = "Số tiền không khớp",
                        RecruiterID = recruiterId,
                        PlanId = planId
                    };
                }

                // Process upgrade
                var upgradeRequest = new SubscriptionUpgradeRequest
                {
                    RecruiterID = recruiterId.Value,
                    PlanId = planId,
                    AmountPaid = model.amountIn,
                    ReferenceCode = model.referenceCode
                };

                bool upgraded = _subscriptionService.ProcessUpgrade(upgradeRequest);

                if (upgraded)
                {
                    PaymentLogger.Info($"Webhook processed successfully. RecruiterID: {recruiterId}, Plan: {planId}");
                    return new WebhookProcessingResult
                    {
                        Success = true,
                        Message = "Nâng cấp thành công",
                        RecruiterID = recruiterId,
                        PlanId = planId
                    };
                }
                else
                {
                    PaymentLogger.Warning($"Upgrade failed. RecruiterID: {recruiterId}, Plan: {planId}");
                    return new WebhookProcessingResult
                    {
                        Success = false,
                        Message = "Nâng cấp thất bại",
                        RecruiterID = recruiterId,
                        PlanId = planId
                    };
                }
            }
            catch (Exception ex)
            {
                PaymentLogger.Error($"Webhook processing failed. ReferenceCode: {model?.referenceCode}", ex);
                return new WebhookProcessingResult
                {
                    Success = false,
                    Message = "Lỗi hệ thống"
                };
            }
        }
    }
}
