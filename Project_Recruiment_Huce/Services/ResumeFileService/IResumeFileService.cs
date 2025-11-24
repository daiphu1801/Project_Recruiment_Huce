using System.Collections.Generic;
using System.Web;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services.ResumeFileService
{
    /// <summary>
    /// Interface cho service quản lý ResumeFile business logic
    /// </summary>
    public interface IResumeFileService
    {
        /// <summary>
        /// Lấy danh sách CV của candidate
        /// </summary>
        ResumeFileListResult GetResumeFiles(int accountId);

        /// <summary>
        /// Upload CV mới
        /// </summary>
        ValidationResult UploadResume(int accountId, HttpPostedFileBase resumeFile, string title, HttpServerUtilityBase server);

        /// <summary>
        /// Xóa CV
        /// </summary>
        ValidationResult DeleteResume(int accountId, int resumeFileId, HttpServerUtilityBase server);

        /// <summary>
        /// Chỉnh sửa tên CV
        /// </summary>
        ValidationResult EditResume(int accountId, int resumeFileId, string title);
    }

    /// <summary>
    /// Kết quả trả về từ GetResumeFiles
    /// </summary>
    public class ResumeFileListResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<ResumeFileViewModel> ResumeFiles { get; set; }
    }
}
