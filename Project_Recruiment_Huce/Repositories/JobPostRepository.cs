using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Repositories
{
    public class JobPostRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public JobPostRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db;
        }

        public IEnumerable<JobPost> GetLatestVisible(int take = 10)
        {
            JobStatusHelper.NormalizeStatuses(_db);
            return _db.JobPosts
                .Where(j => j.Status == JobStatusHelper.Published)
                .OrderByDescending(j => j.PostedAt)
                .Take(take)
                .ToList();
        }

        public JobPost GetWithDetails(int jobId)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.JobPostID == jobId);
            if (job == null) return null;
            return job;
        }

        public IEnumerable<JobPost> Search(string keyword, string industry = null, string employment = null)
        {
            JobStatusHelper.NormalizeStatuses(_db);
            var query = _db.JobPosts.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword));
            }
            if (!string.IsNullOrWhiteSpace(industry))
            {
                var jobIds = _db.JobPostDetails
                    .Where(d => d.Industry == industry)
                    .Select(d => d.JobPostID);
                query = query.Where(j => jobIds.Contains(j.JobPostID));
            }
            if (!string.IsNullOrWhiteSpace(employment))
            {
                query = query.Where(j => j.EmploymentType == employment);
            }
            return query.OrderByDescending(j => j.PostedAt).ToList();
        }
    }
}
