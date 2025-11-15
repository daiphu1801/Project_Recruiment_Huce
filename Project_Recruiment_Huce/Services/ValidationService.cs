using System;
using System.Collections.Generic;
using System.Linq;
using Project_Recruiment_Huce.Models.Jobs;
using Project_Recruiment_Huce.Helpers;

namespace Project_Recruiment_Huce.Services
{
    /// <summary>
    /// Service class for validation logic
    /// Centralizes validation rules across the application
    /// </summary>
    public class ValidationService
    {
        /// <summary>
        /// Validation result class
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public Dictionary<string, string> Errors { get; set; }

            public ValidationResult()
            {
                IsValid = true;
                Errors = new Dictionary<string, string>();
            }

            public void AddError(string field, string message)
            {
                IsValid = false;
                if (!Errors.ContainsKey(field))
                {
                    Errors[field] = message;
                }
            }
        }

        /// <summary>
        /// Validate job create/edit model
        /// </summary>
        public ValidationResult ValidateJobCreate(JobCreateViewModel model)
        {
            var result = new ValidationResult();

            if (model == null)
            {
                result.AddError("Model", "Dữ liệu không hợp lệ");
                return result;
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                result.AddError("Title", "Tiêu đề không được để trống");
            }

            if (string.IsNullOrWhiteSpace(model.Description))
            {
                result.AddError("Description", "Mô tả công việc không được để trống");
            }

            if (string.IsNullOrWhiteSpace(model.Location))
            {
                result.AddError("Location", "Địa điểm không được để trống");
            }

            // Validate salary range
            if (model.SalaryFrom.HasValue && model.SalaryTo.HasValue)
            {
                if (model.SalaryTo < model.SalaryFrom)
                {
                    result.AddError("SalaryTo", "Lương đến phải lớn hơn hoặc bằng lương từ");
                }
            }

            // Validate age range
            if (model.AgeFrom.HasValue && model.AgeTo.HasValue)
            {
                if (model.AgeTo < model.AgeFrom)
                {
                    result.AddError("AgeTo", "Độ tuổi đến phải lớn hơn hoặc bằng độ tuổi từ");
                }

                if (model.AgeFrom < 18)
                {
                    result.AddError("AgeFrom", "Độ tuổi tối thiểu là 18");
                }

                if (model.AgeTo > 100)
                {
                    result.AddError("AgeTo", "Độ tuổi tối đa là 100");
                }
            }

            // Validate deadline
            if (model.ApplicationDeadline.HasValue)
            {
                if (model.ApplicationDeadline.Value < DateTime.Now)
                {
                    result.AddError("ApplicationDeadline", "Hạn nộp hồ sơ phải sau thời điểm hiện tại");
                }
            }

            return result;
        }

        /// <summary>
        /// Validate phone number
        /// </summary>
        public ValidationResult ValidatePhone(string phone, bool required = false)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(phone))
            {
                if (required)
                {
                    result.AddError("Phone", "Số điện thoại không được để trống");
                }
                return result;
            }

            if (!ValidationHelper.IsValidVietnamesePhone(phone))
            {
                result.AddError("Phone", ValidationHelper.GetPhoneErrorMessage());
            }

            return result;
        }

        /// <summary>
        /// Validate email
        /// </summary>
        public ValidationResult ValidateEmail(string email, bool required = false)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(email))
            {
                if (required)
                {
                    result.AddError("Email", "Email không được để trống");
                }
                return result;
            }

            if (!ValidationHelper.IsValidEmail(email))
            {
                result.AddError("Email", "Email không hợp lệ");
            }

            return result;
        }

        /// <summary>
        /// Validate fax number
        /// </summary>
        public ValidationResult ValidateFax(string fax, bool required = false)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(fax))
            {
                if (required)
                {
                    result.AddError("Fax", "Số fax không được để trống");
                }
                return result;
            }

            if (!ValidationHelper.IsValidFax(fax))
            {
                result.AddError("Fax", ValidationHelper.GetFaxErrorMessage());
            }

            return result;
        }

        /// <summary>
        /// Validate password strength
        /// </summary>
        public ValidationResult ValidatePassword(string password, string confirmPassword = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(password))
            {
                result.AddError("Password", "Mật khẩu không được để trống");
                return result;
            }

            if (password.Length < 6)
            {
                result.AddError("Password", "Mật khẩu phải có ít nhất 6 ký tự");
            }

            bool hasLower = password.Any(char.IsLower);
            bool hasUpper = password.Any(char.IsUpper);
            bool hasDigit = password.Any(char.IsDigit);

            if (!hasLower || !hasUpper || !hasDigit)
            {
                result.AddError("Password", "Mật khẩu phải gồm chữ hoa, chữ thường và số");
            }

            if (confirmPassword != null && password != confirmPassword)
            {
                result.AddError("ConfirmPassword", "Mật khẩu và xác nhận mật khẩu không khớp");
            }

            return result;
        }
    }
}

