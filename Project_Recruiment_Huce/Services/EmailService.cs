// File: Services/EmailService.cs
using System;
using System.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Services
{
    public class EmailService
    {
        // Constructor không tham số để Hangfire có thể tạo instance
        public EmailService()
        {
        }

        public virtual void SendInterviewInvitation(InterviewEmailData data)
        {
            try
            {
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
                        <p style='font-size:12px; color:#888;'>Email tự động từ hệ thống tuyển dụng HUCE.</p>
                    </div>";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Tuyen Dung HUCE", ConfigurationManager.AppSettings["SenderEmail"]));
                message.To.Add(new MailboxAddress("", data.CandidateEmail));
                message.Subject = subject;
                message.Body = new BodyBuilder { HtmlBody = content }.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    client.CheckCertificateRevocation = false;

                    client.Connect(
                        ConfigurationManager.AppSettings["SmtpServer"], 
                        int.Parse(ConfigurationManager.AppSettings["Port"]), 
                        SecureSocketOptions.StartTls
                    );
                    
                    client.Authenticate(
                        ConfigurationManager.AppSettings["SenderEmail"], 
                        ConfigurationManager.AppSettings["Password"]
                    );
                    
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                Debug.WriteLine($"=== LỖI GỬI EMAIL ===");
                Debug.WriteLine($"Message: {ex.Message}");
                DebNém lỗi ra để Hangfire biết mà thử lại (Retry)
                throw new Exception($"Lỗi gửi e
}
