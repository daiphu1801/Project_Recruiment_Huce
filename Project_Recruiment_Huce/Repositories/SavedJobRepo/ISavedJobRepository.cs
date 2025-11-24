using System.Collections.Generic;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.SavedJobRepo
{
    /// <summary>
    /// Interface cho repository quản lý SavedJob data access operations
    /// </summary>
    public interface ISavedJobRepository
    {
        /// <summary>
        /// Lấy candidate theo AccountID
        /// </summary>
        Candidate GetCandidateByAccountId(int accountId);

        /// <summary>
        /// Lấy tất cả SavedJobs của candidate với published jobs
        /// </summary>
        List<SavedJob> GetSavedJobs(int candidateId);

        /// <summary>
        /// Lấy JobPost theo ID
        /// </summary>
        JobPost GetJobPost(int jobPostId);

        /// <summary>
        /// Kiểm tra xem job đã được lưu chưa
        /// </summary>
        SavedJob GetSavedJob(int candidateId, int jobPostId);

        /// <summary>
        /// Tạo SavedJob mới
        /// </summary>
        void CreateSavedJob(SavedJob savedJob);

        /// <summary>
        /// Xóa SavedJob
        /// </summary>
        void DeleteSavedJob(SavedJob savedJob);

        /// <summary>
        /// Kiểm tra job đã được lưu chưa (bool)
        /// </summary>
        bool IsJobSaved(int candidateId, int jobPostId);

        /// <summary>
        /// Lưu tất cả thay đổi vào database
        /// </summary>
        void SaveChanges();
    }
}
