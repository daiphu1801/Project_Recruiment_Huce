using System;
using System.Linq;
using System.Net.Mail;
using System.Configuration;
using System.Security.Cryptography;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    public static class PasswordResetHelper
    {
        // Tạo mã 6 ký tự ngẫu nhiên (chữ và số)
        public static string GenerateResetCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using (var random = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[6];
                random.GetBytes(bytes);
                
                var code = new char[6];
                for (int i = 0; i < 6; i++)
                {
                    code[i] = chars[bytes[i] % chars.Length];
                }
                return new string(code);
            }
        }

        // Gửi email chứa mã reset
        public static bool SendResetCodeEmail(string email, string resetCode, string username)
        {
            try
            {
                var smtpHost = ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                var smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
                var smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
                var fromEmail = ConfigurationManager.AppSettings["FromEmail"] ?? smtpUser;

                // Nếu không có cấu hình SMTP, return false
                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
                {
                    return false;
                }

               using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);

                    var mail = new MailMessage
                    {
                        From = new MailAddress(fromEmail, "JobBoard"),
                        Subject = "Mã xác thực đặt lại mật khẩu",
                        Body = $@"
                <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 24px; background: #ffffff; border-radius: 10px; border: 1px solid #e6e6e6;"">
                    <h2 style=""color: #222; margin-bottom: 8px;"">
                        Xin chào <span style=""color:#007bff;"">{username}</span>,
                    </h2>

                    <p style=""font-size: 15px; color:#444; line-height: 1.6;"">
                        Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu của bạn trên hệ thống <strong>JobBoard</strong>.
                    </p>

                    <div style=""background: #f1f5ff; padding: 26px 20px; text-align: center; margin: 28px 0; border-radius: 8px; border: 1px solid #dce6ff;"">
                        <p style=""margin: 0; font-size: 14px; color: #555;"">
                            Mã xác nhận đặt lại mật khẩu của bạn là:
                        </p>
                        <h1 style=""color: #0056d6; font-size: 40px; font-weight: 700; letter-spacing: 8px; margin: 10px 0;"">
                            {resetCode}
                        </h1>
                        <p style=""margin: 0; font-size: 13px; color:#888;"">
                            Mã có hiệu lực trong <strong>15 phút</strong>.
                        </p>
                    </div>

                    <p style=""font-size: 15px; color:#444; line-height: 1.6;"">
                        Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này hoặc liên hệ với đội ngũ hỗ trợ.
                    </p>

                    <hr style=""border: none; border-top: 1px solid #eee; margin: 30px 0;"" />

                    <p style=""color: #999; font-size: 12px; line-height: 1.5; text-align: center;"">
                        Trân trọng,<br />
                        <strong>Đội ngũ JobBoard</strong>
                    </p>
                </div>",
                        IsBodyHtml = true
                    };

                    mail.To.Add(email);
                    client.Send(mail);
                    return true;
                }

            }
            catch (Exception ex)
            {
                // Log error if needed
                System.Diagnostics.Debug.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }

        // Tạo và lưu mã reset vào database
        public static PasswordResetToken CreateResetToken(JOBPORTAL_ENDataContext db, int accountId, string email)
        {
            // Xóa các token cũ chưa dùng của account này
            var oldTokens = db.PasswordResetTokens
                .Where(t => t.AccountID == accountId && t.UsedFlag == 0 && t.ExpiresAt > DateTime.Now)
                .ToList();
            db.PasswordResetTokens.DeleteAllOnSubmit(oldTokens);

            // Tạo token mới
            var resetCode = GenerateResetCode();
            var token = new PasswordResetToken
            {
                AccountID = accountId,
                ResetCode = resetCode,
                Email = email,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(15), // Hết hạn sau 15 phút
                UsedFlag = 0,
                AttemptCount = 0
            };

            db.PasswordResetTokens.InsertOnSubmit(token);
            db.SubmitChanges();

            return token;
        }

        // Xác thực mã reset
        public static PasswordResetToken ValidateResetCode(JOBPORTAL_ENDataContext db, string email, string code)
        {
            var token = db.PasswordResetTokens
                .Where(t => t.Email.ToLower() == email.ToLower() 
                         && t.ResetCode == code 
                         && t.UsedFlag == 0 
                         && t.ExpiresAt > DateTime.Now)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefault();

            if (token != null)
            {
                // Tăng số lần thử
                token.AttemptCount++;
                db.SubmitChanges();
            }

            return token;
        }

        // Đánh dấu token đã sử dụng
        public static void MarkTokenAsUsed(JOBPORTAL_ENDataContext db, int tokenId)
        {
            var token = db.PasswordResetTokens.FirstOrDefault(t => t.TokenID == tokenId);
            if (token != null)
            {
                token.UsedFlag = 1;
                db.SubmitChanges();
            }
        }
    }
}

