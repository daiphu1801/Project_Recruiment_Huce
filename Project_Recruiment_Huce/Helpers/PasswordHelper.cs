using System;
using Microsoft.AspNet.Identity;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để xử lý mã hóa và xác thực mật khẩu sử dụng PBKDF2 (ASP.NET Identity Standard)
    /// </summary>
    public static class PasswordHelper
    {
        // Sử dụng PasswordHasher của ASP.NET Identity (PBKDF2)
        private static readonly PasswordHasher _hasher = new PasswordHasher();

        /// <summary>
        /// Tạo hash mật khẩu theo chuẩn PBKDF2 (đã bao gồm salt tự động)
        /// </summary>
        /// <param name="password">Mật khẩu cần hash</param>
        /// <returns>Chuỗi hash (đã bao gồm salt)</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Mật khẩu không được để trống", nameof(password));
            
            return _hasher.HashPassword(password);
        }

        /// <summary>
        /// Xác thực mật khẩu với hash đã lưu trong database
        /// </summary>
        /// <param name="password">Mật khẩu người dùng nhập</param>
        /// <param name="hashedPassword">Hash lưu trong DB</param>
        /// <returns>true nếu mật khẩu đúng, false nếu sai</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            var result = _hasher.VerifyHashedPassword(hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }
}