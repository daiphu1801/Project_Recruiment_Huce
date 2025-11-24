using System;
using System.Security.Cryptography;
using System.Text;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để xử lý mã hóa và xác thực mật khẩu
    /// Sử dụng SHA256 với salt để bảo mật mật khẩu người dùng
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Tạo salt ngẫu nhiên cho mật khẩu (32 bytes)
        /// Salt được sử dụng để tăng cường bảo mật khi hash mật khẩu
        /// </summary>
        /// <returns>Chuỗi Base64 của salt</returns>
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hash mật khẩu với salt sử dụng thuật toán SHA256
        /// Kết hợp password + salt trước khi hash để tăng cường bảo mật
        /// </summary>
        /// <param name="password">Mật khẩu gốc cần hash</param>
        /// <param name="salt">Salt để kết hợp với mật khẩu</param>
        /// <returns>Chuỗi hash của mật khẩu (64 ký tự hex)</returns>
        public static string HashPassword(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            if (string.IsNullOrEmpty(salt))
                salt = GenerateSalt();

            // Kết hợp password và salt
            string passwordWithSalt = password + salt;
            
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Xác thực mật khẩu với hash và salt
        /// So sánh hash của input password với hash đã lưu
        /// </summary>
        /// <param name="password">Mật khẩu cần xác thực</param>
        /// <param name="hashedPassword">Hash đã lưu trong database</param>
        /// <param name="salt">Salt đã lưu trong database</param>
        /// <returns>True nếu mật khẩu đúng, false nếu sai</returns>
        public static bool VerifyPassword(string password, string hashedPassword, string salt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            string hashOfInput = HashPassword(password, salt);
            return hashOfInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// [Phương thức cũ] Hash mật khẩu không sử dụng salt
        /// Chỉ dùng để tương thích ngược với dữ liệu cũ
        /// </summary>
        /// <param name="password">Mật khẩu cần hash</param>
        /// <returns>Chuỗi hash của mật khẩu</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// [Phương thức cũ] Xác thực mật khẩu không sử dụng salt
        /// Chỉ dùng để tương thích ngược với dữ liệu cũ
        /// </summary>
        /// <param name="password">Mật khẩu cần xác thực</param>
        /// <param name="hashedPassword">Hash đã lưu</param>
        /// <returns>True nếu đúng, false nếu sai</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                return false;

            string hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }

        internal static string HashPassword(object password, string salt)
        {
            throw new NotImplementedException();
        }
    }
}

