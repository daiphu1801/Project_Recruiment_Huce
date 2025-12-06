using Project_Recruiment_Huce.Models.Home;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Giao diện service xử lý logic nghiệp vụ cho tính năng liên hệ
    /// </summary>
    public interface IContactService
    {
        /// <summary>
        /// Validate và gửi tin nhắn liên hệ
        /// </summary>
        /// <param name="model">ViewModel chứa thông tin liên hệ</param>
        /// <returns>Kết quả validation</returns>
        ValidationResult SendContactMessage(ContactViewModel model);

        /// <summary>
        /// Kiểm tra email có tồn tại trong hệ thống không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <returns>True nếu email tồn tại</returns>
        bool IsEmailRegistered(string email);

        /// <summary>
        /// Lấy thông tin liên hệ của công ty
        /// </summary>
        /// <returns>Thông tin liên hệ</returns>
        ContactInfoViewModel GetContactInfo();
    }
}
