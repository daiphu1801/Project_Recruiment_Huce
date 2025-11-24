using System;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.JobRepo
{
    /// <summary>
    /// Interface định nghĩa các phương thức truy xuất dữ liệu tin tuyển dụng
    /// Áp dụng Repository Pattern cho JobPost và JobPostDetail entities
    /// </summary>
    public interface IJobRepository
    {
        // JobPost operations
        JobPost GetJobPostById(int jobPostId);
        JobPost GetJobPostByIdAndRecruiterId(int jobPostId, int recruiterId);
        void CreateJobPost(JobPost jobPost);
        void UpdateJobPost(JobPost jobPost);
        
        // JobPostDetail operations
        JobPostDetail GetJobPostDetailByJobPostId(int jobPostId);
        void CreateJobPostDetail(JobPostDetail jobPostDetail);
        void UpdateJobPostDetail(JobPostDetail jobPostDetail);
        
        // Recruiter operations
        Recruiter GetRecruiterById(int recruiterId);
        
        // Company operations
        Company GetCompanyById(int companyId);
        
        // Job code generation
        string GenerateNextJobCode();
        
        // Application checking
        int GetPendingApplicationsCount(int jobPostId);
        
        // Validation helpers
        bool IsJobOwnedByRecruiter(int jobPostId, int recruiterId);
        
        // Save changes
        void SaveChanges();
    }
}
