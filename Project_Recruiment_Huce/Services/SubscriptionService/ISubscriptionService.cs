using System;
using System.Collections.Generic;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Payment;

namespace Project_Recruiment_Huce.Services.SubscriptionService
{
    /// <summary>
    /// Service interface for subscription business logic
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Get all available subscription plans
        /// </summary>
        List<SubscriptionPlan> GetAvailablePlans();

        /// <summary>
        /// Get plan by ID
        /// </summary>
        SubscriptionPlan GetPlanById(string planId);

        /// <summary>
        /// Process subscription upgrade
        /// </summary>
        bool ProcessUpgrade(SubscriptionUpgradeRequest request);

        /// <summary>
        /// Get subscription status for a recruiter
        /// </summary>
        SubscriptionStatusDto GetSubscriptionStatus(int recruiterId);

        /// <summary>
        /// Check if recruiter has active subscription
        /// </summary>
        bool HasActiveSubscription(int recruiterId);

        /// <summary>
        /// Parse payment content to extract RecruiterID and PlanID
        /// Supports formats: "UPGRADE 123 Monthly", "UP123 Monthly", "BankAPINotify-UP123 Monthly-..."
        /// </summary>
        (int? recruiterId, string planId) ParsePaymentContent(string content);

        /// <summary>
        /// Validate payment amount matches plan price
        /// </summary>
        bool ValidatePaymentAmount(string planId, decimal amountPaid);
    }
}
