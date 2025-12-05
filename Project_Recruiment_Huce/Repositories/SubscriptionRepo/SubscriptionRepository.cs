using System;
using System.Linq;
using Project_Recruiment_Huce.Infrastructure;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Payment;

namespace Project_Recruiment_Huce.Repositories.SubscriptionRepo
{
    /// <summary>
    /// Repository for subscription data access
    /// </summary>
    public class SubscriptionRepository : ISubscriptionRepository
    {
        /// <summary>
        /// Get recruiter by ID
        /// </summary>
        public Recruiter GetRecruiterById(int recruiterId)
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                return db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
            }
        }

        /// <summary>
        /// Update recruiter subscription
        /// </summary>
        public void UpdateRecruiterSubscription(Recruiter recruiter)
        {
            using (var db = DbContextFactory.Create())
            {
                var existingRecruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiter.RecruiterID);
                if (existingRecruiter != null)
                {
                    existingRecruiter.SubscriptionType = recruiter.SubscriptionType;
                    existingRecruiter.SubscriptionExpiryDate = recruiter.SubscriptionExpiryDate;
                    existingRecruiter.FreeJobPostCount = recruiter.FreeJobPostCount;
                    db.SubmitChanges();
                }
            }
        }

        /// <summary>
        /// Check if transaction already exists (idempotency)
        /// </summary>
        public bool TransactionExists(string referenceCode)
        {
            if (string.IsNullOrEmpty(referenceCode))
                return false;

            using (var db = DbContextFactory.CreateReadOnly())
            {
                return db.SePayTransactions.Any(t => t.ReferenceCode == referenceCode);
            }
        }

        /// <summary>
        /// Save SePay transaction
        /// </summary>
        public void SaveTransaction(SePayTransaction transaction)
        {
            using (var db = DbContextFactory.Create())
            {
                db.SePayTransactions.InsertOnSubmit(transaction);
                db.SubmitChanges();
            }
        }

        /// <summary>
        /// Get subscription status
        /// </summary>
        public SubscriptionStatusDto GetSubscriptionStatus(int recruiterId)
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
                if (recruiter == null)
                    return null;

                return new SubscriptionStatusDto
                {
                    CurrentPlan = recruiter.SubscriptionType ?? "Free",
                    ExpiryDate = recruiter.SubscriptionExpiryDate,
                    FreeJobPostCount = recruiter.FreeJobPostCount,
                    IsUpgraded = !string.IsNullOrEmpty(recruiter.SubscriptionType) && recruiter.SubscriptionType != "Free"
                };
            }
        }

        /// <summary>
        /// Check if recruiter has active subscription
        /// </summary>
        public bool HasActiveSubscription(int recruiterId)
        {
            using (var db = DbContextFactory.CreateReadOnly())
            {
                var recruiter = db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
                if (recruiter == null || string.IsNullOrEmpty(recruiter.SubscriptionType) || recruiter.SubscriptionType == "Free")
                    return false;

                // Lifetime has no expiry
                if (recruiter.SubscriptionType == "Lifetime")
                    return true;

                // Monthly check expiry
                return recruiter.SubscriptionExpiryDate.HasValue && recruiter.SubscriptionExpiryDate.Value > DateTime.Now;
            }
        }
    }
}
