using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_Recruiment_Huce.Models
{
    public class SubscriptionViewModel
    {
        public string CurrentPlan { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int FreeJobPostCount { get; set; }
        public List<SubscriptionPlan> Plans { get; set; }
    }

    public class SubscriptionPlan
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Duration { get; set; }
    }
}
