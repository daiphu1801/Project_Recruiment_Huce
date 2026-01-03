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

        public Account Create(string username, string email, string phone, string role, string passwordHash, string salt, string fullName = null)
        {
            var account = new Account
            {
                Username = username,
                Email = email,
                Phone = phone,
                Role = role,
                PasswordHash = passwordHash,
                FullName = !string.IsNullOrWhiteSpace(fullName) ? fullName : username, // Default to username if fullName not provided
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

        // ============================================================
        // GOOGLE LOGIN REPOSITORY METHODS
        // ============================================================

        /// <summary>
        /// Tìm account theo GoogleId
        /// </summary>
        public Account FindByGoogleId(string googleId)
        {
            if (string.IsNullOrWhiteSpace(googleId)) return null;
            return _db.Accounts.FirstOrDefault(a => a.GoogleId == googleId && a.ActiveFlag == 1);
        }

        /// <summary>
        /// Tìm account theo Email (chính xác, không phân biệt hoa thường)
        /// </summary>
        public Account FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var emailLower = email.ToLower();
            return _db.Accounts.FirstOrDefault(a => a.Email.ToLower() == emailLower && a.ActiveFlag == 1);
        }

        /// <summary>
        /// Tạo tài khoản Google mới (chỉ tạo Account, không tạo Candidate)
        /// </summary>
        public Account CreateGoogleAccount(string email, string fullName, string googleId, string username)
        {
            var account = new Account
            {
                Username = username,
                Email = email,
                FullName = fullName,
                GoogleId = googleId,
                IsGoogleAccount = true,
                PasswordHash = null, // Google account không có password local
                Role = "Candidate", // CHECK constraint yêu cầu: 'Admin', 'Recruiter', 'Candidate'
                CreatedAt = DateTime.Now,
                ActiveFlag = 1,
                Phone = null,
                PhotoID = null
            };

            _db.Accounts.InsertOnSubmit(account);
            _db.SubmitChanges();
            return account;
        }

        /// <summary>
        /// Tạo Candidate profile cho account
        /// </summary>
        public void CreateCandidateProfile(int accountId, string fullName, string email)
        {
            var candidate = new Candidate
            {
                AccountID = accountId,
                FullName = fullName,
                Email = email,
                BirthDate = null, // User sẽ cập nhật sau
                Gender = "Nam", // CHECK constraint: 'Nữ' hoặc 'Nam', default 'Nam' vì chưa biết
                Phone = null,
                Address = null,
                Summary = null,
                PhotoID = null,
                ApplicationEmail = email,
                CreatedAt = DateTime.Now,
                ActiveFlag = 1
            };

            _db.Candidates.InsertOnSubmit(candidate);
            _db.SubmitChanges();
        }

        /// <summary>
        /// Lưu ảnh đại diện từ URL Google
        /// </summary>
        public ProfilePhoto SaveProfilePhoto(string photoUrl, string fileName)
        {
            if (string.IsNullOrWhiteSpace(photoUrl)) return null;

            var photo = new ProfilePhoto
            {
                FileName = fileName,
                FilePath = photoUrl, // Lưu URL từ Google
                FileFormat = "jpg",
                FileSizeKB = null,
                UploadedAt = DateTime.Now
            };

            _db.ProfilePhotos.InsertOnSubmit(photo);
            _db.SubmitChanges();
            return photo;
        }

        /// <summary>
        /// Cập nhật PhotoID cho Account
        /// </summary>
        public void UpdateAccountPhotoId(int accountId, int photoId)
        {
            var account = _db.Accounts.FirstOrDefault(a => a.AccountID == accountId);
            if (account != null)
            {
                account.PhotoID = photoId;
                _db.SubmitChanges();
            }
        }

        /// <summary>
        /// Generate username duy nhất từ email
        /// Ví dụ: john.doe@gmail.com -> johndoe hoặc johndoe1234
        /// </summary>
        public string GenerateUniqueUsername(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            // Lấy phần trước @ và loại bỏ ký tự đặc biệt
            var baseUsername = email.Split('@')[0]
                .Replace(".", "")
                .Replace("_", "")
                .Replace("-", "")
                .ToLower();

            // Giới hạn độ dài
            if (baseUsername.Length > 50)
                baseUsername = baseUsername.Substring(0, 50);

            // Kiểm tra trùng
            if (!UsernameExists(baseUsername))
                return baseUsername;

            // Nếu trùng, thêm random số 4 chữ số
            var random = new Random();
            for (int i = 0; i < 100; i++) // Thử 100 lần
            {
                var suffix = random.Next(1000, 9999);
                var newUsername = baseUsername + suffix;
                if (!UsernameExists(newUsername))
                    return newUsername;
            }

            // Fallback: timestamp
            return baseUsername + DateTime.Now.Ticks.ToString().Substring(8);
        }
        
    }
}
