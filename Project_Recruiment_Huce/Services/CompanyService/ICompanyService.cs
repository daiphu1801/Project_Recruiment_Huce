using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models.Companies;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Interface định nghĩa các phương thức cho CompanyService
    /// </summary>
    public interface ICompanyService
    {
        /// <summary>
        /// Lấy thông tin công ty của recruiter để hiển thị trên view manage
        /// </summary>
        /// <param name="accountId">ID tài khoản</param>
        /// <returns>ViewModel chứa thông tin công ty</returns>
        CompanyManageViewModel GetCompanyManageViewModel(int accountId);

        /// <summary>
        /// Tạo mới hoặc cập nhật thông tin công ty
        /// </summary>
        /// <param name="viewModel">Dữ liệu công ty từ form</param>
        /// <param name="accountId">ID tài khoản</param>
        /// <param name="newPhotoId">ID ảnh mới (nếu có)</param>
        /// <returns>Kết quả validation</returns>
        ValidationResult SaveOrUpdateCompany(CompanyManageViewModel viewModel, int accountId, int? newPhotoId = null);
    }
}
