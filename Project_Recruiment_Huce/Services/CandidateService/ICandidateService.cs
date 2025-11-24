using System.Collections.Generic;
using System.Web;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Candidates;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services.CandidateService
{
    /// <summary>
    /// Interface cho service quản lý candidate business logic
    /// </summary>
    public interface ICandidateService
    {
        /// <summary>
        /// Lấy hoặc tạo candidate cho account (dùng cho CandidatesManage GET)
        /// </summary>
        CandidateManageViewModel GetOrCreateCandidate(int accountId, string username);

        /// <summary>
        /// Validate và cập nhật candidate profile
        /// </summary>
        ValidationResult ValidateAndUpdateProfile(
            CandidateManageViewModel viewModel,
            int accountId,
            HttpPostedFileBase avatar,
            HttpServerUtilityBase server);

        /// <summary>
        /// Lấy danh sách applications của candidate với pagination
        /// </summary>
        ApplicationListResult GetApplicationsList(int accountId, int pageNumber, int pageSize);

        /// <summary>
        /// Lấy dữ liệu cho form Apply
        /// </summary>
        ApplyFormResult GetApplyFormData(int accountId, int jobPostId, HttpServerUtilityBase server);

        /// <summary>
        /// Validate và submit application
        /// </summary>
        ValidationResult SubmitApplication(
            ApplicationApplyViewModel viewModel,
            int accountId,
            HttpPostedFileBase newResumeFile,
            HttpServerUtilityBase server);
    }

    /// <summary>
    /// Kết quả trả về từ GetApplicationsList
    /// </summary>
    public class ApplicationListResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public List<ApplicationListItemViewModel> Applications { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    /// <summary>
    /// Kết quả trả về từ GetApplyFormData
    /// </summary>
    public class ApplyFormResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public ApplicationApplyViewModel ViewModel { get; set; }
    }
}
