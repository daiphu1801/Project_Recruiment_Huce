using System.Collections.Generic;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories.ResumeFileRepo
{
    /// <summary>
    /// Interface cho repository quản lý ResumeFile data access operations
    /// </summary>
    public interface IResumeFileRepository
    {
        /// <summary>
        /// Lấy candidate theo AccountID
        /// </summary>
        Candidate GetCandidateByAccountId(int accountId);

        /// <summary>
        /// Lấy tất cả ResumeFiles của candidate
        /// </summary>
        List<ResumeFile> GetResumeFilesByCandidateId(int candidateId);

        /// <summary>
        /// Lấy ResumeFile theo ID và CandidateID (security check)
        /// </summary>
        ResumeFile GetResumeFile(int resumeFileId, int candidateId);

        /// <summary>
        /// Tạo ResumeFile mới
        /// </summary>
        void CreateResumeFile(ResumeFile resumeFile);

        /// <summary>
        /// Xóa ResumeFile
        /// </summary>
        void DeleteResumeFile(ResumeFile resumeFile);

        /// <summary>
        /// Cập nhật tên ResumeFile
        /// </summary>
        void UpdateResumeFileName(ResumeFile resumeFile, string newFileName);

        /// <summary>
        /// Lưu tất cả thay đổi vào database
        /// </summary>
        void SaveChanges();
    }
}
