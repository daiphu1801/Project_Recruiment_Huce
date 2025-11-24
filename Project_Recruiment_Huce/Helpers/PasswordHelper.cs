using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNet.Identity;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để xử lý mã hóa và xác thực mật khẩu
    /// Hỗ trợ migration từ SHA256 (Legacy) sang PBKDF2 (ASP.NET Identity Standard)
    /// </summary>
    public static class PasswordHelper
    {
        // Sử dụng PasswordHasher của ASP.NET Identity (PBKDF2)
        private static readonly PasswordHasher _hasher = new PasswordHasher();

        /// <summary>
        /// Kết quả xác thực mật khẩu
        /// </summary>
        public enum VerifyResult
        {
            Success,              // Mật khẩu đúng (PBKDF2 format)
            SuccessRehashNeeded,  // Mật khẩu đúng (SHA256 format) - cần update
            Failed                // Mật khẩu sai
        }

        /// <summary>
        /// Tạo hash mật khẩu theo chuẩn mới (PBKDF2)
        /// </summary>
        /// <param name="password">Mật khẩu cần hash</param>
        /// <returns>Chuỗi hash (đã bao gồm salt)</returns>
        public static string HashPassword(string password)
        {
            return _hasher.HashPassword(password);
        }

        /// <summary>
        /// Xác thực mật khẩu với hỗ trợ migration tự động (SHA256 → PBKDF2)
        /// </summary>
        /// <param name="password">Mật khẩu người dùng nhập</param>
        /// <param name="dbHash">Hash lưu trong DB</param>
        /// <param name="dbSalt">Salt lưu trong DB (null nếu dùng PBKDF2)</param>
        /// <returns>Kết quả xác thực</returns>
        public static VerifyResult VerifyPasswordV2(string password, string dbHash, string dbSalt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(dbHash))
                return VerifyResult.Failed;

            // 1. Thử verify theo PBKDF2 (format mới)
            var result = _hasher.VerifyHashedPassword(dbHash, password);
            if (result == PasswordVerificationResult.Success)
                return VerifyResult.Success;

            // 2. Nếu fail và có Salt → verify theo SHA256 (format cũ)
            if (result == PasswordVerificationResult.Failed && !string.IsNullOrEmpty(dbSalt))
            {
                string legacyHash = HashPasswordLegacy(password, dbSalt);
                if (string.Equals(legacyHash, dbHash, StringComparison.OrdinalIgnoreCase))
                    return VerifyResult.SuccessRehashNeeded;
            }

            return VerifyResult.Failed;
        }

        #region Legacy Support (SHA256) - For Migration Only

        /// <summary>
        /// Hash password theo format cũ (SHA256 + Salt) - chỉ dùng cho verify legacy passwords
        /// </summary>
        private static string HashPasswordLegacy(string password, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt))
                return null;

            string passwordWithSalt = password + salt;
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (byte b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        /// <summary>
        /// Generate salt - chỉ dùng cho testing/migration scripts
        /// </summary>
        [Obsolete("Salt không còn cần thiết với PBKDF2. Chỉ dùng cho migration scripts.")]
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        #endregion
    }
}

