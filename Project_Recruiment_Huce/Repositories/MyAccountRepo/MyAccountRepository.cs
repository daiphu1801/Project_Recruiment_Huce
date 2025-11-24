using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Triển khai IMyAccountRepository sử dụng JOBPORTAL_ENDataContext
    /// </summary>
    public class MyAccountRepository : IMyAccountRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public MyAccountRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public Account GetAccountById(int accountId)
        {
            return _db.Accounts.FirstOrDefault(a => a.AccountID == accountId);
        }

        public Candidate GetCandidateByAccountId(int accountId)
        {
            return _db.Candidates.FirstOrDefault(c => c.AccountID == accountId);
        }

        public Recruiter GetRecruiterByAccountId(int accountId)
        {
            return _db.Recruiters.FirstOrDefault(r => r.AccountID == accountId);
        }

        public void UpdatePassword(int accountId, string passwordHash, string salt)
        {
            if (!_db.ObjectTrackingEnabled)
                throw new InvalidOperationException("Không thể cập nhật mật khẩu: context không cho phép ghi. Sử dụng DbContextFactory.Create() để lấy context có thể ghi.");

            var account = GetAccountById(accountId);
            if (account == null)
                throw new InvalidOperationException($"Không tìm thấy tài khoản với ID {accountId}");

            account.PasswordHash = passwordHash;
            // Salt column đã bị xóa khỏi database
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }
    }
}
