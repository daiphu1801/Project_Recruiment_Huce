using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để validate các trường input từ người dùng
    /// Cung cấp các phương thức kiểm tra định dạng và tính duy nhất của dữ liệu
    /// </summary>
    public static class ValidationHelper
    {
        // Pattern để kiểm tra số điện thoại Việt Nam
        // Hỗ trợ: 0xxxxxxxxx (10 số), +84xxxxxxxxx hoặc 84xxxxxxxxx (11 số)
        // Đầu số phổ biến: 03, 05, 07, 08, 09
        private static readonly Regex VietnamesePhoneRegex = new Regex(
            @"^(0[35789][0-9]{8}|(\+84|84)[35789][0-9]{8})$",
            RegexOptions.Compiled
        );

        // Pattern để kiểm tra số fax (tương tự số điện thoại cố định Việt Nam)
        // Hỗ trợ: 0[2-9]xxxxxxxx (10 số bắt đầu bằng 02x cho Hà Nội, 028 cho TP.HCM, etc.)
        private static readonly Regex FaxRegex = new Regex(
            @"^(0[2-9][0-9]{8}|(\+84|84)[2-9][0-9]{8})$",
            RegexOptions.Compiled
        );

        /// <summary>
        /// Kiểm tra định dạng số điện thoại Việt Nam
        /// </summary>
        /// <param name="phone">Số điện thoại cần kiểm tra</param>
        /// <returns>True nếu hợp lệ, false nếu không</returns>
        public static bool IsValidVietnamesePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Loại bỏ khoảng trắng, dấu gạch ngang và dấu ngoặc
            var cleaned = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Kiểm tra pattern
            if (!VietnamesePhoneRegex.IsMatch(cleaned))
                return false;

            // Validation bổ sung: phải có 10 hoặc 11 chữ số
            var digitsOnly = Regex.Replace(cleaned, @"[^\d]", "");
            return digitsOnly.Length == 10 || digitsOnly.Length == 11;
        }

        /// <summary>
        /// Kiểm tra định dạng số fax
        /// </summary>
        /// <param name="fax">Số fax cần kiểm tra</param>
        /// <returns>True nếu hợp lệ, false nếu không</returns>
        public static bool IsValidFax(string fax)
        {
            if (string.IsNullOrWhiteSpace(fax))
                return false;

            // Loại bỏ khoảng trắng, dấu gạch ngang và dấu ngoặc
            var cleaned = Regex.Replace(fax, @"[\s\-\(\)]", "");

            // Kiểm tra pattern
            if (!FaxRegex.IsMatch(cleaned))
                return false;

            // Validation phải có 10 hoặc 11 chữ số
            var digitsOnly = Regex.Replace(cleaned, @"[^\d]", "");
            return digitsOnly.Length == 10 || digitsOnly.Length == 11;
        }

        /// <summary>
        /// Kiểm tra định dạng email
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <returns>True nếu hợp lệ, false nếu không</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra email có duy nhất trong bảng Accounts hay không
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="excludeAccountId">ID của Account được loại trừ khỏi kiểm tra (cho trường hợp update)</param>
        /// <returns>True nếu duy nhất, false nếu trùng lặp</returns>
        public static bool IsEmailUniqueInAccounts(string email, int? excludeAccountId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var emailLower = email.ToLowerInvariant();
                if (excludeAccountId.HasValue)
                {
                    return !db.Accounts.Any(a => a.Email.ToLower() == emailLower && a.AccountID != excludeAccountId.Value);
                }
                return !db.Accounts.Any(a => a.Email.ToLower() == emailLower);
            }
        }

        /// <summary>
        /// Checks if company email is unique in Companies table
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="excludeCompanyId">Company ID to exclude from check (for updates)</param>
        /// <returns>True if unique, false if duplicate</returns>
        public static bool IsCompanyEmailUnique(string email, int? excludeCompanyId = null)
        {
            if (string.IsNullOrWhiteSpace(email))
                return true;

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var emailLower = email.ToLowerInvariant();
                if (excludeCompanyId.HasValue)
                {
                    return !db.Companies.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower && c.CompanyID != excludeCompanyId.Value);
                }
                return !db.Companies.Any(c => c.CompanyEmail != null && c.CompanyEmail.ToLower() == emailLower);
            }
        }

        /// <summary>
        /// Checks if phone number is unique in Companies table
        /// Normalizes phone numbers from database before comparison to handle different formats
        /// </summary>
        /// <param name="phone">Phone to check (should already be normalized)</param>
        /// <param name="excludeCompanyId">Company ID to exclude from check (for updates)</param>
        /// <returns>True if unique, false if duplicate</returns>
        public static bool IsCompanyPhoneUnique(string phone, int? excludeCompanyId = null)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            var normalizedPhone = NormalizePhone(phone);
            if (string.IsNullOrWhiteSpace(normalizedPhone))
                return true;

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var companiesWithPhone = db.Companies
                    .Where(c => c.Phone != null)
                    .ToList()
                    .Where(c => !string.IsNullOrWhiteSpace(c.Phone))
                    .ToList();

                foreach (var company in companiesWithPhone)
                {
                    if (excludeCompanyId.HasValue && company.CompanyID == excludeCompanyId.Value)
                        continue;

                    var dbPhoneNormalized = SafeNormalizePhone(company.Phone);
                    if (dbPhoneNormalized != null && 
                        string.Equals(dbPhoneNormalized, normalizedPhone, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Checks if phone number is unique in Accounts table
        /// Normalizes phone numbers from database before comparison to handle different formats
        /// </summary>
        /// <param name="phone">Phone to check (should already be normalized)</param>
        /// <param name="excludeAccountId">Account ID to exclude from check (for updates)</param>
        /// <returns>True if unique, false if duplicate</returns>
        public static bool IsAccountPhoneUnique(string phone, int? excludeAccountId = null)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            var normalizedPhone = NormalizePhone(phone);
            if (string.IsNullOrWhiteSpace(normalizedPhone))
                return true;

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                var accountsWithPhone = db.Accounts
                    .Where(a => a.Phone != null)
                    .ToList()
                    .Where(a => !string.IsNullOrWhiteSpace(a.Phone))
                    .ToList();

                foreach (var account in accountsWithPhone)
                {
                    if (excludeAccountId.HasValue && account.AccountID == excludeAccountId.Value)
                        continue;

                    var dbPhoneNormalized = SafeNormalizePhone(account.Phone);
                    if (dbPhoneNormalized != null && 
                        string.Equals(dbPhoneNormalized, normalizedPhone, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Normalizes phone number to standard format (0xxxxxxxxx)
        /// </summary>
        /// <param name="phone">Phone number to normalize</param>
        /// <returns>Normalized phone number</returns>
        public static string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return phone;

            var cleaned = Regex.Replace(phone, @"[^\d]", "");
            if (string.IsNullOrWhiteSpace(cleaned))
                return phone;

            if (cleaned.StartsWith("84") && (cleaned.Length == 11 || cleaned.Length == 10))
            {
                cleaned = "0" + cleaned.Substring(2);
            }
            else if (!cleaned.StartsWith("0") && cleaned.Length >= 9)
            {
                cleaned = "0" + cleaned;
            }

            return cleaned;
        }

        /// <summary>
        /// Safely normalizes phone number, returns null if normalization fails
        /// </summary>
        /// <param name="phone">Phone number to normalize</param>
        /// <returns>Normalized phone number or null if invalid</returns>
        private static string SafeNormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            try
            {
                return NormalizePhone(phone);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets error message for invalid phone number
        /// </summary>
        public static string GetPhoneErrorMessage()
        {
            return "Số điện thoại không hợp lệ. Vui lòng nhập số điện thoại Việt Nam 10-11 số (ví dụ: 0912345678, 84123456789).";
        }

        /// <summary>
        /// Gets error message for invalid fax number
        /// </summary>
        public static string GetFaxErrorMessage()
        {
            return "Số fax không hợp lệ. Vui lòng nhập số fax hợp lệ (ví dụ: 02412345678).";
        }
    }
}

