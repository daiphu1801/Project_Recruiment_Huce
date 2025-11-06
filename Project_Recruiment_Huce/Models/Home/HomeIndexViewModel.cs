using System.Collections.Generic;
using Project_Recruiment_Huce.Models.Jobs;

namespace Project_Recruiment_Huce.Models.Home
{
    public class HomeIndexViewModel
    {
        public int TotalCandidates { get; set; }
        public int TotalJobPosts { get; set; }
        public int TotalHiredJobs { get; set; }
        public int TotalCompanies { get; set; }
        public List<JobListingItemViewModel> RecentJobs { get; set; }
        public int TotalJobsCount { get; set; }
    }
}

