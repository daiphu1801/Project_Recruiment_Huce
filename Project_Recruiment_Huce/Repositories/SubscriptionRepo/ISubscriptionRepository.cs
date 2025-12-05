using System;
using System.Collections.Generic;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Payment;

namespace Project_Recruiment_Huce.Repositories.SubscriptionRepo
{
    /// <summary>
    /// Repository interface for subscription data access
    /// </summary>
    public interface ISubscriptionRepository
    {
        /// <summary>
        /// Get recruiter by ID
        /// </summary>
        Recruiter GetRecruiterById(int recruiterId);

        /// <summary>
        /// Update recruiter subscription
        /// </summary>
        void UpdateRecruiterSubscription(Recruiter recruiter);

        /// <summary>
        /// Check if transaction already exists (idempotency)
        /// </summary>
        bool TransactionExists(string referenceCode);

        /// <summary>
        /// Save SePay transaction
        /// </summary>
        void SaveTransaction(SePayTransaction transaction);

        /// <summary>
        /// Get subscription status
        /// </summary>
        SubscriptionStatusDto GetSubscriptionStatus(int recruiterId);

        /// <summary>
        /// Check if recruiter has active subscription
        /// </summary>
        bool HasActiveSubscription(int recruiterId);
    }
}
