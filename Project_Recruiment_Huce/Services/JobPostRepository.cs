using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.DbContext;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Services
{
    public class JobPostRepository
    {
        private readonly RecruitmentDbContext _db;

        public JobPostRepository(RecruitmentDbContext db)
        {
            _db = db;
        }

        public IEnumerable<JobPost> GetLatestVisible(int take = 10)
        {
            return _db.JobPosts
                .Where(j => j.Status == "Visible")
                .OrderByDescending(j => j.PostedAt)
                .Take(take)
                .ToList();
        }

        public JobPost GetWithDetails(int jobId)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.JobId == jobId);
            if (job == null) return null;
            // consumers can query details when needed
            return job;
        }

        public IEnumerable<JobPost> Search(string keyword, string industry = null, string employment = null)
        {
            var query = _db.JobPosts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword));
            }
            if (!string.IsNullOrWhiteSpace(industry))
            {
                var jobIds = _db.JobPostDetails
                    .Where(d => d.Industry == industry)
                    .Select(d => d.JobId);
                query = query.Where(j => jobIds.Contains(j.JobId));
            }
            if (!string.IsNullOrWhiteSpace(employment))
            {
                query = query.Where(j => j.Employment == employment);
            }
            return query.OrderByDescending(j => j.PostedAt).ToList();
        }
    }
}


