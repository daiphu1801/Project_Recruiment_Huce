//email sync helper
using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để tạo profile (Candidate/Recruiter) tự động khi tạo Account
    /// Lưu ý: Email trong Account và profile được quản lý độc lập
    /// - Account.Email: Đăng nhập, quên mật khẩu (không thể sửa)
    /// - Candidate.Email / Recruiter.CompanyEmail: Email liên lạc (có thể sửa)
    /// </summary>
    public static class EmailSyncHelper
    {
        /// <summary>
        /// Tự động tạo profile Candidate hoặc Recruiter khi tạo Account mới
        /// Email trong profile để trống, cho phép người dùng tự cập nhật
        /// </summary>
        /// <param name="db">Database context</param>
        /// <param name="accountId">ID của Account vừa tạo</param>
        /// <param name="role">Vai trò (Candidate hoặc Recruiter)</param>
        /// <param name="fullName">Tên đầy đủ (tùy chọn)</param>
        public static void CreateProfile(JOBPORTAL_ENDataContext db, int accountId, string role, string fullName = null)
        {
            var account = db.Accounts.FirstOrDefault(a => a.AccountID == accountId);
            if (account == null) return;

            // Tạo Candidate profile nếu role là Candidate và chưa có profile
            if (role == "Candidate")
            {
                var existingCandidate = db.Candidates.FirstOrDefault(c => c.AccountID == accountId);
                if (existingCandidate == null)
                {
                    var candidate = new Candidate
                    {
                        AccountID = accountId,
                        //FullName = fullName ?? account.Username ?? "Chưa cập nhật",
                        FullName = fullName ?? string.Empty,
                        Gender = "Nam", // Default
                        Email = null, // Email liên lạc - để trống, user sẽ tự cập nhật
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    db.Candidates.InsertOnSubmit(candidate);
                }
            }
            // Tạo Recruiter profile nếu role là Recruiter và chưa có profile
            else if (role == "Recruiter")
            {
                var existingRecruiter = db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
                if (existingRecruiter == null)
                {
                    var recruiter = new Recruiter
                    {
                        AccountID = accountId,
                        //FullName = fullName ?? account.Username ?? "Chưa cập nhật",
                        FullName = fullName ?? string.Empty,
                        CompanyEmail = null, // Email liên lạc - để trống, user sẽ tự cập nhật
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1,
                        SubscriptionType = "Free", // Gói miễn phí mặc định
                        FreeJobPostCount = 3, // 3 bài đăng miễn phí
                        MonthlyJobPostCount = 0,
                        MonthlyCVViewCount = 0,
                        MonthlyEmailInviteCount = 0,
                        LastResetDate = DateTime.Now // Khởi tạo ngày reset để tránh SqlDateTime overflow
                    };
                    db.Recruiters.InsertOnSubmit(recruiter);
                }
            }
        }
    }
}

