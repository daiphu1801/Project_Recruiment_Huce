using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Models.Accounts;
using Project_Recruiment_Huce.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Project_Recruiment_Huce.Repositories
{
    /// <summary>
    /// Repository cho các thao tác data access với Account
    /// Cung cấp CRUD và các query phục vụ authentication/validation
    /// </summary>
    public class AccountRepository : IAccountRepository
    {
        private readonly JOBPORTAL_ENDataContext _db;

        public AccountRepository(JOBPORTAL_ENDataContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));

        }

        /// <summary>
        /// Lấy account theo ID
        /// </summary>
        public Account GetById(int accountId)
        {
            return _db.Accounts.FirstOrDefault(a => a.AccountID == accountId);
        }

        /// <summary>
        /// Tìm account theo username HOẶC email
        /// </summary>
        public Account FindByUsernameOrEmail(string emailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername)) return null;
            return _db.Accounts.FirstOrDefault(a =>
                (a.Username == emailOrUsername || a.Email == emailOrUsername));
        }

        public bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return _db.Accounts.Any(a => a.Username == username);
        }

        public bool EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var lower = email.ToLower();
            return _db.Accounts.Any(a => a.Email.ToLower() == lower);
        }

        public bool PhoneExistsNormalized(string normalizedPhone)
        {
            if (string.IsNullOrWhiteSpace(normalizedPhone)) return false;
            return _db.Accounts.Any(a => (a.Phone ?? string.Empty) == normalizedPhone);
        }

        public Account Create(string username, string email, string phone, string role, string passwordHash, string salt)
        {
            var account = new Account
            {
                Username = username,
                Email = email,
                Phone = phone,
                Role = role,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now,
                ActiveFlag = 1
            };
            _db.Accounts.InsertOnSubmit(account);
            _db.SubmitChanges();
            return account;
        }

        public void UpdatePassword(int accountId, string passwordHash, string salt)
        {
            var account = _db.Accounts.FirstOrDefault(a => a.AccountID == accountId);
            if (account == null)
                throw new InvalidOperationException($"Không tìm thấy tài khoản với ID {accountId}");

            if (!_db.ObjectTrackingEnabled)
                throw new InvalidOperationException("Không thể cập nhật mật khẩu: ObjectTrackingEnabled đang tắt. Vui lòng sử dụng DbContextFactory.Create() để lấy context có thể ghi (không sử dụng CreateReadOnly()).");

            account.PasswordHash = passwordHash;
            // Salt column đã bị xóa khỏi database
        }

        public void SaveChanges()
        {
            _db.SubmitChanges();
        }

        public void UpdateAccount(Account account)
        {
            // account should be attached/tracked if retrieved from this context
            if (account == null) throw new ArgumentNullException(nameof(account));
            if (!_db.ObjectTrackingEnabled)
                throw new InvalidOperationException("Không thể cập nhật thông tin tài khoản: ObjectTrackingEnabled đang tắt. Vui lòng sử dụng DbContextFactory.Create() để lấy context có thể ghi.");

            // No-op: changes are tracked on the entity, caller should call SaveChanges()
        }

        public void UpdatePasswordResetToken(PasswordResetToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            if (!_db.ObjectTrackingEnabled)
                throw new InvalidOperationException("Không thể cập nhật token: ObjectTrackingEnabled đang tắt. Vui lòng sử dụng DbContextFactory.Create() để lấy context có thể ghi.");

            // Token entity is tracked if it was fetched from this context; just ensure values set and saved by caller
        }

        public PasswordResetToken GetPasswordResetToken(string emailLower, string resetCodeUpper)
        {
            if (string.IsNullOrWhiteSpace(emailLower) || string.IsNullOrWhiteSpace(resetCodeUpper)) return null;
            var now = DateTime.Now;  // Sử dụng local time để khớp với CreatedAt/ExpiresAt trong PasswordResetHelper
            return _db.PasswordResetTokens
                      .FirstOrDefault(t => t.Email.ToLower() == emailLower
                                           && t.ResetCode == resetCodeUpper
                                           && t.UsedFlag == 0
                                           && t.ExpiresAt > now);
        }
        public void CreateGoogleProfile(string email, string firstName, string lastName, int userId, string FullName, int userType, string Avatar, DateTime BirthDate)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(FullName))
            {
                throw new ArgumentException("UserId và FullName không hợp lệ.");
            }


            // UserType: 1 = Candidate (Ứng viên), 2 = Recruiter (Nhà tuyển dụng)
            if (userType == 1)
            {
                var candidate = new Candidate
                {
                    // FIX LỖI Account_ID: Dùng AccountID (hoặc AccountId)
                    AccountID = userId,

                    // FIX LỖI FullName: Dùng Full_Name (hoặc FullName, tùy theo designer.cs của bạn)
                    FullName = FullName,

                    Avatar = Avatar,

                    BirthDate = DateTime.Now.AddYears(-20),
                    Phone = null,
                    Address = null
                };
                _db.Candidates.InsertOnSubmit(candidate);
            }
            else if (userType == 2)
            {
                var recruiter = new Recruiter
                {
                    // FIX LỖI Account_ID: Dùng AccountID
                    AccountID = userId,

                    // FIX LỖI FullName: Dùng Full_Name
                    FullName = FullName,

                    Avatar = Avatar,
                    PositionTitle = "HR/Recruiter",


                };
                _db.Recruiters.InsertOnSubmit(recruiter);
            }
            else
            {
                throw new ArgumentException($"Loại người dùng không hợp lệ: {userType}");
            }

            _db.SubmitChanges();



        }
        
    }
}
