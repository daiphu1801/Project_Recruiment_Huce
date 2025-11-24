using System.Collections.Generic;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services.SavedJobService
{
    /// <summary>
    /// Interface cho service quản lý SavedJob business logic
    /// </summary>
    public interface ISavedJobService
    {
        /// <summary>
        /// Lấy danh sách công việc đã lưu với search, filter, sort, pagination
        /// </summary>
        SavedJobListResult GetSavedJobs(
            int accountId, 
            string keyword, 
            string location, 
            string sortBy, 
            int pageNumber, 
            int pageSize);

        /// <summary>
        /// Lưu công việc
        /// </summary>
        ValidationResult SaveJob(int accountId, int jobPostId);

        /// <summary>
        /// Bỏ lưu công việc
        /// </summary>
        ValidationResult UnsaveJob(int accountId, int jobPostId);

        /// <summary>
        /// Kiểm tra công việc đã được lưu chưa
        /// </summary>
        bool IsJobSaved(int accountId, int jobPostId);
    }

    /// <summary>
    /// Kết quả trả về từ GetSavedJobs
    /// </summary>
    public class SavedJobListResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<SavedJobViewModel> SavedJobs { get; set; }
        public List<string> Locations { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string Keyword { get; set; }
        public string Location { get; set; }
        public string SortBy { get; set; }
    }
}
