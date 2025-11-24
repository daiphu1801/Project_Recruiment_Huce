using System;
using System.Configuration;
using System.Data.Linq;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.JobRepo
{
    /// <summary>
    /// Repository triển khai truy xuất dữ liệu cho JobPost
    /// Sử dụng LINQ-to-SQL với DataLoadOptions cho eager loading
    /// </summary>
    public class JobRepository : IJobRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public JobRepository(bool readOnly = false)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            _db = new JOBPORTAL_ENDataContext(connectionString);

            if (readOnly)
            {
                _db.ObjectTrackingEnabled = false;
            }

            // Configure eager loading for common scenarios
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<JobPost>(j => j.Company);
            loadOptions.LoadWith<JobPost>(j => j.Recruiter);
            loadOptions.LoadWith<JobPost>(j => j.JobPostDetails);
            loadOptions.LoadWith<Recruiter>(r => r.Company);
            _db.LoadOptions = loadOptions;
        }

        public JobPost GetJobPostById(int jobPostId)
        {
            return _db.JobPosts.FirstOrDefault(j => j.JobPostID == jobPostId);
        }

        public JobPost GetJobPostByIdAndRecruiterId(int jobPostId, int recruiterId)
        {
            return _db.JobPosts.FirstOrDefault(j => j.JobPostID == jobPostId && j.RecruiterID == recruiterId);
        }

        public void CreateJobPost(JobPost jobPost)
        {
            if (jobPost == null)
                throw new ArgumentNullException(nameof(jobPost));

            _db.JobPosts.InsertOnSubmit(jobPost);
        }

        public void UpdateJobPost(JobPost jobPost)
        {
            if (jobPost == null)
                throw new ArgumentNullException(nameof(jobPost));

            var existing = _db.JobPosts.FirstOrDefault(j => j.JobPostID == jobPost.JobPostID);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy JobPost với ID {jobPost.JobPostID}");

            // Update properties for change tracking
            existing.Title = jobPost.Title;
            existing.Description = jobPost.Description;
            existing.Requirements = jobPost.Requirements;
            existing.SalaryFrom = jobPost.SalaryFrom;
            existing.SalaryTo = jobPost.SalaryTo;
            existing.SalaryCurrency = jobPost.SalaryCurrency;
            existing.Location = jobPost.Location;
            existing.EmploymentType = jobPost.EmploymentType;
            existing.ApplicationDeadline = jobPost.ApplicationDeadline;
            existing.Status = jobPost.Status;
            existing.UpdatedAt = DateTime.Now;
        }

        public JobPostDetail GetJobPostDetailByJobPostId(int jobPostId)
        {
            return _db.JobPostDetails.FirstOrDefault(jd => jd.JobPostID == jobPostId);
        }

        public void CreateJobPostDetail(JobPostDetail jobPostDetail)
        {
            if (jobPostDetail == null)
                throw new ArgumentNullException(nameof(jobPostDetail));

            _db.JobPostDetails.InsertOnSubmit(jobPostDetail);
        }

        public void UpdateJobPostDetail(JobPostDetail jobPostDetail)
        {
            if (jobPostDetail == null)
                throw new ArgumentNullException(nameof(jobPostDetail));

            var existing = _db.JobPostDetails.FirstOrDefault(jd => jd.JobPostID == jobPostDetail.JobPostID);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy JobPostDetail với JobPostID {jobPostDetail.JobPostID}");

            // Update properties
            existing.Industry = jobPostDetail.Industry;
            existing.Major = jobPostDetail.Major;
            existing.YearsExperience = jobPostDetail.YearsExperience;
            existing.DegreeRequired = jobPostDetail.DegreeRequired;
            existing.Skills = jobPostDetail.Skills;
            existing.Headcount = jobPostDetail.Headcount;
            existing.GenderRequirement = jobPostDetail.GenderRequirement;
            existing.AgeFrom = jobPostDetail.AgeFrom;
            existing.AgeTo = jobPostDetail.AgeTo;
            existing.Status = jobPostDetail.Status;
        }

        public Recruiter GetRecruiterById(int recruiterId)
        {
            return _db.Recruiters.FirstOrDefault(r => r.RecruiterID == recruiterId);
        }

        public Company GetCompanyById(int companyId)
        {
            return _db.Companies.FirstOrDefault(c => c.CompanyID == companyId);
        }

        public string GenerateNextJobCode()
        {
            var lastJob = _db.JobPosts.OrderByDescending(j => j.JobPostID).FirstOrDefault();
            int nextNumber = 1;

            if (lastJob != null && !string.IsNullOrEmpty(lastJob.JobCode))
            {
                string code = lastJob.JobCode.ToUpper();
                if (code.StartsWith("JOB"))
                {
                    string numStr = code.Substring(3);
                    if (int.TryParse(numStr, out int lastNum))
                    {
                        nextNumber = lastNum + 1;
                    }
                }
                else
                {
                    nextNumber = lastJob.JobPostID + 1;
                }
            }

            return $"JOB{nextNumber:D4}";
        }

        public int GetPendingApplicationsCount(int jobPostId)
        {
            var pendingStatuses = new[] { "Under review", "Interview", "Offered" };
            return _db.Applications
                .Count(a => a.JobPostID == jobPostId && 
                           a.Status != null && 
                           pendingStatuses.Contains(a.Status));
        }

        public bool IsJobOwnedByRecruiter(int jobPostId, int recruiterId)
        {
            return _db.JobPosts.Any(j => j.JobPostID == jobPostId && j.RecruiterID == recruiterId);
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }
    }
}
