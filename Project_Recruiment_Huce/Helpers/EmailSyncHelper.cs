//email sync helper
using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để đồng bộ email giữa Accounts và Candidates/Recruiters
    /// </summary>
    public static class EmailSyncHelper
    {
        // NOTE: Không còn đồng bộ email giữa Account và profile
        // Email trong Account.Email: dùng để đăng nhập và xử lý token (quên mật khẩu) - không thể sửa từ user
        // Email trong Candidate.Email / Recruiter.CompanyEmail: email liên lạc, có thể sửa độc lập

        /// <summary>
        /// Tự động tạo profile Candidate hoặc Recruiter khi tạo Account
        /// NOTE: Không set email từ Account - email trong profile là email liên lạc riêng
        /// </summary>
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
                        ActiveFlag = 1
                    };
                    db.Recruiters.InsertOnSubmit(recruiter);
                }
            }
        }
    }
}

