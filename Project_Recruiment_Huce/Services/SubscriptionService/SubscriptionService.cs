using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Payment;
using Project_Recruiment_Huce.Repositories.SubscriptionRepo;

namespace Project_Recruiment_Huce.Services.SubscriptionService
{
    /// <summary>
    /// Service for subscription business logic
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _repository;

        public SubscriptionService() : this(new SubscriptionRepository())
        {
        }
        private readonly List<SubscriptionPlan> _plans;

        public SubscriptionService(ISubscriptionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            
            // Define subscription plans
            _plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan 
                { 
                    Id = "Monthly", 
                    Name = "Gói Tháng", 
                    Price = 25000, 
                    Duration = "1 Tháng", 
                    Description = "Đăng tin không giới hạn trong 30 ngày." 
                },
                new SubscriptionPlan 
                { 
                    Id = "Lifetime", 
                    Name = "Gói Trọn Đời", 
                    Price = 250000, 
                    Duration = "Vĩnh viễn", 
                    Description = "Đăng tin không giới hạn trọn đời." 
                }
            };
        }

        /// <summary>
        /// Get all available subscription plans
        /// </summary>
        public List<SubscriptionPlan> GetAvailablePlans()
        {
            return _plans;
        }

        /// <summary>
        /// Get plan by ID
        /// </summary>
        public SubscriptionPlan GetPlanById(string planId)
        {
            return _plans.FirstOrDefault(p => p.Id.Equals(planId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Process subscription upgrade
        /// </summary>
        public bool ProcessUpgrade(SubscriptionUpgradeRequest request)
        {
            if (request == null)
            {
                PaymentLogger.Warning("ProcessUpgrade: Request is null");
                return false;
            }

            var recruiter = _repository.GetRecruiterById(request.RecruiterID);
            if (recruiter == null)
            {
                PaymentLogger.Warning($"ProcessUpgrade: Recruiter not found - ID: {request.RecruiterID}");
                return false;
            }

            var plan = GetPlanById(request.PlanId);
            if (plan == null)
            {
                PaymentLogger.Warning($"ProcessUpgrade: Plan not found - {request.PlanId}");
                return false;
            }

            // Validate amount (allow some tolerance or amountIn=0 if content is valid)
            if (request.AmountPaid > 0 && request.AmountPaid < plan.Price)
            {
                PaymentLogger.Warning($"ProcessUpgrade: Amount mismatch. Required: {plan.Price}, Paid: {request.AmountPaid}");
                return false;
            }

            var oldSubscription = recruiter.SubscriptionType;
            var oldExpiry = recruiter.SubscriptionExpiryDate;

            // Update subscription
            recruiter.SubscriptionType = plan.Id;

            if (plan.Id == "Monthly")
            {
                // Extend if already monthly, else set new
                if (recruiter.SubscriptionExpiryDate.HasValue && recruiter.SubscriptionExpiryDate > DateTime.Now)
                {
                    recruiter.SubscriptionExpiryDate = recruiter.SubscriptionExpiryDate.Value.AddDays(30);
                }
                else
                {
                    recruiter.SubscriptionExpiryDate = DateTime.Now.AddDays(30);
                }
            }
            else if (plan.Id == "Lifetime")
            {
                recruiter.SubscriptionExpiryDate = null; // No expiry
            }

            // Reset free count
            recruiter.FreeJobPostCount = 0;

            _repository.UpdateRecruiterSubscription(recruiter);

            PaymentLogger.Info($"Upgrade successful. RecruiterID: {request.RecruiterID}, "
                + $"Old: {oldSubscription} (Expiry: {oldExpiry}), "
                + $"New: {recruiter.SubscriptionType} (Expiry: {recruiter.SubscriptionExpiryDate})");

            return true;
        }

        /// <summary>
        /// Get subscription status for a recruiter
        /// </summary>
        public SubscriptionStatusDto GetSubscriptionStatus(int recruiterId)
        {
            return _repository.GetSubscriptionStatus(recruiterId);
        }

        /// <summary>
        /// Check if recruiter has active subscription
        /// </summary>
        public bool HasActiveSubscription(int recruiterId)
        {
            return _repository.HasActiveSubscription(recruiterId);
        }

        /// <summary>
        /// Parse payment content to extract RecruiterID and PlanID
        /// Supports formats:
        /// - "UPGRADE 123 Monthly"
        /// - "UP123 Monthly"
        /// - "BankAPINotify 109808362433-UP37 Monthly-CHUYEN TIEN-..."
        /// </summary>
        public (int? recruiterId, string planId) ParsePaymentContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return (null, null);

            PaymentLogger.Info($"Parsing payment content: {content}");

            // Try regex pattern to extract "UP{ID} {Plan}" from anywhere in the string
            var match = Regex.Match(content, @"UP(\d+)\s+(\w+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int recruiterId))
                {
                    string planId = match.Groups[2].Value;
                    PaymentLogger.Info($"Parsed via regex: RecruiterID={recruiterId}, PlanId={planId}");
                    return (recruiterId, planId);
                }
            }

            // Fallback: Try splitting by spaces and finding UPGRADE/UP keyword
            var parts = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int upgradeIndex = Array.FindIndex(parts, p =>
                p.Equals("UPGRADE", StringComparison.OrdinalIgnoreCase) ||
                p.StartsWith("UP", StringComparison.OrdinalIgnoreCase));

            if (upgradeIndex >= 0 && parts.Length > upgradeIndex + 1)
            {
                string firstPart = parts[upgradeIndex];
                string recruiterIdStr = "";
                string planId = "";

                // Check if format is "UP123" (short format)
                if (firstPart.StartsWith("UP", StringComparison.OrdinalIgnoreCase) && firstPart.Length > 2)
                {
                    recruiterIdStr = firstPart.Substring(2);
                    planId = parts.Length > upgradeIndex + 1 ? parts[upgradeIndex + 1] : "";
                }
                // Format is "UPGRADE 123" (full format)
                else
                {
                    recruiterIdStr = parts.Length > upgradeIndex + 1 ? parts[upgradeIndex + 1] : "";
                    planId = parts.Length > upgradeIndex + 2 ? parts[upgradeIndex + 2] : "";
                }

                if (int.TryParse(recruiterIdStr, out int recruiterId))
                {
                    PaymentLogger.Info($"Parsed via split: RecruiterID={recruiterId}, PlanId={planId}");
                    return (recruiterId, planId);
                }
            }

            PaymentLogger.Warning($"Failed to parse payment content: {content}");
            return (null, null);
        }

        /// <summary>
        /// Validate payment amount matches plan price
        /// </summary>
        public bool ValidatePaymentAmount(string planId, decimal amountPaid)
        {
            var plan = GetPlanById(planId);
            if (plan == null)
                return false;

            // Allow amountIn=0 (SePay API limitation) or exact/greater amount
            return amountPaid == 0 || amountPaid >= plan.Price;
        }
    }
}
