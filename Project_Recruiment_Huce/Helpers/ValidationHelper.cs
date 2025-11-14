using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Configuration;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class for validating various input fields
    /// </summary>
    public static class ValidationHelper
    {
        // Regex pattern for Vietnamese phone numbers
        // Supports: 0xxxxxxxxx (10 số), +84xxxxxxxxx hoặc 84xxxxxxxxx (11 số)
        // Số điện thoại Việt Nam: 10 số (0 + 9 số) hoặc 11 số (84 + 9 số)
        // Đầu số phổ biến: 03, 05, 07, 08, 09
        private static readonly Regex VietnamesePhoneRegex = new Regex(
            @"^(0[35789][0-9]{8}|(\+84|84)[35789][0-9]{8})$",
            RegexOptions.Compiled
        );

        // Regex pattern for fax numbers (similar to phone but can have extensions)
        private static readonly Regex FaxRegex = new Regex(
            @"^(0|\+84|84)(3[2-9]|5[6|8|9]|7[0|6-9]|8[1-6|8|9]|9[0-4|6-9])[0-9]{7}(?:[-\s]?ext[-\s]?[0-9]{1,4})?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Validates Vietnamese phone number format
        /// </summary>
        /// <param name="phone">Phone number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidVietnamesePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // Remove spaces, dashes, and parentheses
            var cleaned = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Check regex pattern
            if (!VietnamesePhoneRegex.IsMatch(cleaned))
                return false;

            // Additional validation: must be 10 or 11 digits total
            var digitsOnly = Regex.Replace(cleaned, @"[^\d]", "");
            return digitsOnly.Length == 10 || digitsOnly.Length == 11;
        }

        /// <summary>
        /// Validates fax number format
        /// </summary>
        /// <param name="fax">Fax number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidFax(string fax)
        {
            if (string.IsNullOrWhiteSpace(fax))
                return false;

            // Remove spaces, dashes
            var cleaned = Regex.Replace(fax, @"[\s\-]", "");

            return FaxRegex.IsMatch(cleaned);
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        /// <param name="email">Email to validate</param>
        /// <returns>True if valid, false otherwise</returns>
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
        /// Checks if email is unique in Accounts table
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <param name="excludeAccountId">Account ID to exclude from check (for updates)</param>
        /// <returns>True if unique, false if duplicate</returns>
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
        /// </summary>
        /// <param name="phone">Phone to check</param>
        /// <param name="excludeCompanyId">Company ID to exclude from check (for updates)</param>
        /// <returns>True if unique, false if duplicate</returns>
        public static bool IsCompanyPhoneUnique(string phone, int? excludeCompanyId = null)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                if (excludeCompanyId.HasValue)
                {
                    return !db.Companies.Any(c => c.Phone != null && c.Phone == phone && c.CompanyID != excludeCompanyId.Value);
                }
                return !db.Companies.Any(c => c.Phone != null && c.Phone == phone);
            }
        }

        /// <summary>
        /// Checks if phone number is unique in Accounts table
        /// </summary>
        /// <param name="phone">Phone to check</param>
        /// <param name="excludeAccountId">Account ID to exclude from check (for updates)</param>
        /// <returns>True if unique, false if duplicate</returns>
        public static bool IsAccountPhoneUnique(string phone, int? excludeAccountId = null)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            using (var db = new JOBPORTAL_ENDataContext(ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString))
            {
                if (excludeAccountId.HasValue)
                {
                    return !db.Accounts.Any(a => a.Phone != null && a.Phone == phone && a.AccountID != excludeAccountId.Value);
                }
                return !db.Accounts.Any(a => a.Phone != null && a.Phone == phone);
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

            // Remove spaces, dashes, parentheses
            var cleaned = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // Convert +84 or 84 to 0
            if (cleaned.StartsWith("+84"))
            {
                cleaned = "0" + cleaned.Substring(3);
            }
            else if (cleaned.StartsWith("84") && cleaned.Length >= 10)
            {
                cleaned = "0" + cleaned.Substring(2);
            }

            return cleaned;
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

