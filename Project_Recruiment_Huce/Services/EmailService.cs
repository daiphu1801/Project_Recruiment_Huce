// File: Services/EmailService.cs
using System;
using System.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Project_Recruiment_Huce.Models; // Để dùng được InterviewEmailData ở trên

namespace Project_Recruiment_Huce.Services
{
    public class EmailService
    {
        public void SendInterviewInvitation(InterviewEmailData data)
        {
            try
            {
                // Validate email addresses
                var senderEmail = ConfigurationManager.AppSettings["FromEmail"];
                if (string.IsNullOrWhiteSpace(senderEmail))
                {
                    throw new Exception("Chưa cấu hình FromEmail trong Web.config");
                }

                if (string.IsNullOrWhiteSpace(data.CandidateEmail))
                {
                    throw new Exception("Email ứng viên không được để trống");
                }

                string subject = $"[Thư Mời Phỏng Vấn] - {data.JobTitle}";

                // Nội dung HTML
                string content = $@"
                    <div style='font-family:Arial, sans-serif; padding:20px; border:1px solid #ddd;'>
                        <h2 style='color:#0056b3;'>THƯ MỜI PHỎNG VẤN</h2>
                        <p>Chào bạn <strong>{data.CandidateName}</strong>,</p>
                        <p>Chúng tôi trân trọng mời bạn tham gia buổi phỏng vấn vị trí <strong style='color:#e74c3c;'>{data.JobTitle}</strong>.</p>
                        
                        <table style='background:#f9f9f9; width:100%; padding:15px; margin:15px 0;'>
                            <tr><td><strong>Thời gian:</strong></td><td>{data.InterviewTime} ngày {data.InterviewDate:dd/MM/yyyy}</td></tr>
                            <tr><td><strong>Hình thức:</strong></td><td>{data.InterviewType}</td></tr>
                            <tr><td><strong>Địa điểm:</strong></td><td>{data.Location}</td></tr>
                            <tr><td><strong>Người PV:</strong></td><td>{data.Interviewer}</td></tr>
                        </table>

                        <p><strong>Yêu cầu mang theo:</strong> {data.RequiredDocuments}</p>
                        <p><strong>Lưu ý:</strong> {data.AdditionalNotes}</p>
                        <p>Vui lòng phản hồi email này để xác nhận tham gia.</p>
                        <hr/>
                        <p style='font-size:12px; color:#888;'>Email tự động từ hệ thống tuyển dụng JobBoard.</p>
                    </div>";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Tuyen Dung HUCE", senderEmail));
                message.To.Add(new MailboxAddress(data.CandidateName ?? "", data.CandidateEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = content }.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Quan trọng: Bỏ qua lỗi chứng chỉ SSL khi chạy localhost
                    client.CheckCertificateRevocation = false;

                    var smtpServer = ConfigurationManager.AppSettings["SmtpHost"];
                    var portStr = ConfigurationManager.AppSettings["SmtpPort"];
                    var password = ConfigurationManager.AppSettings["SmtpPassword"];

                    if (string.IsNullOrWhiteSpace(smtpServer))
                    {
                        throw new Exception("Chưa cấu hình SmtpHost trong Web.config");
                    }

                    if (string.IsNullOrWhiteSpace(portStr) || !int.TryParse(portStr, out int port))
                    {
                        throw new Exception("Chưa cấu hình SmtpPort hợp lệ trong Web.config");
                    }

                    if (string.IsNullOrWhiteSpace(password))
                    {
                        throw new Exception("Chưa cấu hình SmtpPassword trong Web.config");
                    }

                    client.Connect(smtpServer, port, SecureSocketOptions.StartTls);
                    client.Authenticate(senderEmail, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // Ném lỗi ra để Hangfire biết mà thử lại (Retry)
                throw new Exception("Lỗi gửi mail: " + ex.Message);
            }
        }
    }
}