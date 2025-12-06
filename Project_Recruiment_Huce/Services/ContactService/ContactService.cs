using System;
using System.Configuration;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Home;
using Project_Recruiment_Huce.Repositories;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service triển khai logic nghiệp vụ cho tính năng liên hệ
    /// </summary>
    public class ContactService : IContactService
    {
        private readonly IContactRepository _contactRepository;
        private readonly JOBPORTAL_ENDataContext _db;

        public ContactService(IContactRepository contactRepository, JOBPORTAL_ENDataContext db)
        {
            _contactRepository = contactRepository;
            _db = db;
        }

        /// <inheritdoc/>
        public ValidationResult SendContactMessage(ContactViewModel model)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate input
            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                result.IsValid = false;
                result.Errors.Add("FirstName", "Vui lòng nhập họ");
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                result.IsValid = false;
                result.Errors.Add("LastName", "Vui lòng nhập tên");
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                result.IsValid = false;
                result.Errors.Add("Email", "Vui lòng nhập email");
            }
            else if (!IsValidEmail(model.Email))
            {
                result.IsValid = false;
                result.Errors.Add("Email", "Email không hợp lệ");
            }
            // Optional: Check if email exists in system (based on activity diagram)
            // Uncomment if you want to require registered emails
            // else if (!IsEmailRegistered(model.Email))
            // {
            //     result.IsValid = false;
            //     result.Errors.Add("Email", "Email không tồn tại trong hệ thống. Vui lòng nhập lại.");
            // }

            if (string.IsNullOrWhiteSpace(model.Subject))
            {
                result.IsValid = false;
                result.Errors.Add("Subject", "Vui lòng nhập chủ đề");
            }

            if (string.IsNullOrWhiteSpace(model.Message))
            {
                result.IsValid = false;
                result.Errors.Add("Message", "Vui lòng nhập nội dung tin nhắn");
            }

            // If validation passed, create the contact message
            if (result.IsValid)
            {
                try
                {
                    _contactRepository.Create(
                        model.FirstName.Trim(),
                        model.LastName.Trim(),
                        model.Email.Trim().ToLowerInvariant(),
                        model.Subject.Trim(),
                        model.Message.Trim()
                    );

                    result.Message = "Tin nhắn của bạn đã được gửi thành công. Chúng tôi sẽ phản hồi sớm nhất có thể!";
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add("General", "Có lỗi xảy ra khi gửi tin nhắn. Vui lòng thử lại sau.");
                    System.Diagnostics.Debug.WriteLine($"Error sending contact message: {ex.Message}");
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public bool IsEmailRegistered(string email)
        {
            return _contactRepository.EmailExistsInSystem(email);
        }

        /// <inheritdoc/>
        public ContactInfoViewModel GetContactInfo()
        {
            // Có thể lấy từ cấu hình hoặc database
            return new ContactInfoViewModel
            {
                Address = ConfigurationManager.AppSettings["Company:Address"] ?? "Số 2 Phạm Văn Đồng, Phường Dịch Vọng, Quận Cầu Giấy, Hà Nội",
                Phone = ConfigurationManager.AppSettings["Company:Phone"] ?? "+84 24 3869 4242",
                Email = ConfigurationManager.AppSettings["Company:Email"] ?? "contact@huce.edu.vn",
                WorkingHours = ConfigurationManager.AppSettings["Company:WorkingHours"] ?? "Thứ 2 - Thứ 6: 8:00 - 17:00"
            };
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }
    }
}
