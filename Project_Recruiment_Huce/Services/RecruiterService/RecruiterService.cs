using System;
using System.IO;
using System.Linq;
using System.Web;
using Project_Recruiment_Huce.Helpers;
using Project_Recruiment_Huce.Models;
using Project_Recruiment_Huce.Repositories.RecruiterRepo;

namespace Project_Recruiment_Huce.Services.RecruiterService
{
    /// <summary>
    /// Service implementation for Recruiter profile operations
    /// </summary>
    public class RecruiterService : IRecruiterService
    {
        private readonly IRecruiterRepository _repository;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public RecruiterService(IRecruiterRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public RecruiterProfileResult GetOrCreateProfile(int accountId, string userName)
        {
            try
            {
                var recruiter = _repository.GetByAccountId(accountId);
                
                if (recruiter == null)
                {
                    // Create new recruiter profile
                    recruiter = new Recruiter
                    {
                        AccountID = accountId,
                        FullName = userName,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    _repository.Create(recruiter);
                    _repository.SaveChanges();
                }

                return new RecruiterProfileResult
                {
                    Success = true,
                    Recruiter = recruiter
                };
            }
            catch (Exception ex)
            {
                return new RecruiterProfileResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi tải hồ sơ: {ex.Message}"
                };
            }
        }

        public ValidationResult ValidateRecruiter(Recruiter recruiter, int accountId)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate and normalize phone
            var phone = (recruiter.Phone ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (!ValidationHelper.IsValidVietnamesePhone(phone))
                {
                    result.IsValid = false;
                    result.Errors["Phone"] = ValidationHelper.GetPhoneErrorMessage();
                }
                else
                {
                    phone = ValidationHelper.NormalizePhone(phone);
                    
                    // Check uniqueness
                    if (!_repository.IsPhoneUnique(phone, accountId))
                    {
                        result.IsValid = false;
                        result.Errors["Phone"] = "Số điện thoại này đã được sử dụng bởi tài khoản hoặc hồ sơ khác.";
                    }
                }
            }
            else
            {
                phone = null;
            }
            result.NormalizedPhone = phone;

            // Validate and normalize company email
            var companyEmail = (recruiter.CompanyEmail ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(companyEmail))
            {
                if (!ValidationHelper.IsValidEmail(companyEmail))
                {
                    result.IsValid = false;
                    result.Errors["CompanyEmail"] = "Email không hợp lệ.";
                }
                else
                {
                    // Check uniqueness in Accounts table
                    if (!_repository.IsCompanyEmailUnique(companyEmail, accountId))
                    {
                        result.IsValid = false;
                        result.Errors["CompanyEmail"] = "Email này đã được sử dụng bởi tài khoản khác.";
                    }
                }
            }
            else
            {
                companyEmail = null;
            }
            result.NormalizedEmail = companyEmail;

            return result;
        }

        public UpdateRecruiterResult UpdateProfile(int accountId, Recruiter recruiter, HttpPostedFileBase avatar, Func<string, string> serverMapPath)
        {
            var result = new UpdateRecruiterResult();

            try
            {
                // Validate recruiter data
                var validation = ValidateRecruiter(recruiter, accountId);
                if (!validation.IsValid)
                {
                    result.Success = false;
                    result.ErrorMessage = "Vui lòng kiểm tra lại thông tin. Có lỗi trong form.";
                    result.Errors = validation.Errors;
                    return result;
                }

                // Get or create recruiter
                var existingRecruiter = _repository.GetByAccountId(accountId);
                if (existingRecruiter == null)
                {
                    // Create new recruiter
                    existingRecruiter = new Recruiter
                    {
                        AccountID = accountId,
                        FullName = recruiter.FullName,
                        PositionTitle = recruiter.PositionTitle,
                        CompanyEmail = validation.NormalizedEmail,
                        Phone = validation.NormalizedPhone,
                        CompanyID = recruiter.CompanyID,
                        CreatedAt = DateTime.Now,
                        ActiveFlag = 1
                    };
                    _repository.Create(existingRecruiter);
                }
                else
                {
                    // Update existing recruiter
                    if (!string.IsNullOrWhiteSpace(recruiter.FullName))
                    {
                        existingRecruiter.FullName = recruiter.FullName;
                    }
                    existingRecruiter.PositionTitle = recruiter.PositionTitle;
                    existingRecruiter.CompanyEmail = validation.NormalizedEmail;
                    existingRecruiter.Phone = validation.NormalizedPhone;
                    existingRecruiter.CompanyID = recruiter.CompanyID;
                }

                // Handle avatar upload
                if (avatar != null && avatar.ContentLength > 0)
                {
                    var uploadResult = HandleAvatarUpload(accountId, avatar, serverMapPath);
                    if (!uploadResult.Success)
                    {
                        result.Success = false;
                        result.ErrorMessage = uploadResult.ErrorMessage;
                        return result;
                    }
                }

                // Save changes
                _repository.SaveChanges();

                result.Success = true;
                result.SuccessMessage = "Cập nhật hồ sơ thành công!";
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"Lỗi khi cập nhật hồ sơ: {ex.Message}";
                return result;
            }
        }

        private UploadResult HandleAvatarUpload(int accountId, HttpPostedFileBase avatar, Func<string, string> serverMapPath)
        {
            var ext = Path.GetExtension(avatar.FileName)?.ToLower();
            
            if (ext == null || !AllowedExtensions.Contains(ext))
            {
                return new UploadResult
                {
                    Success = false,
                    ErrorMessage = "Chỉ chấp nhận file ảnh (JPG, PNG, GIF, WEBP)."
                };
            }

            try
            {
                // Get account
                var account = _repository.GetAccountById(accountId);
                if (account == null)
                {
                    return new UploadResult
                    {
                        Success = false,
                        ErrorMessage = "Không tìm thấy tài khoản."
                    };
                }

                // Delete old photo if exists
                if (account.PhotoID.HasValue)
                {
                    var oldPhoto = _repository.GetProfilePhotoById(account.PhotoID.Value);
                    if (oldPhoto != null)
                    {
                        var oldFilePath = serverMapPath("~" + oldPhoto.FilePath);
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                        _repository.DeleteProfilePhoto(oldPhoto);
                    }
                }

                // Save new avatar
                var uploadsRoot = serverMapPath("~/Content/uploads/recruiter/");
                if (!Directory.Exists(uploadsRoot))
                {
                    Directory.CreateDirectory(uploadsRoot);
                }

                var safeFileName = $"avatar_{accountId}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
                var physicalPath = Path.Combine(uploadsRoot, safeFileName);
                avatar.SaveAs(physicalPath);

                var relativePath = $"~/Content/uploads/recruiter/{safeFileName}";
                var photo = new ProfilePhoto
                {
                    FileName = safeFileName,
                    FilePath = relativePath,
                    FileSizeKB = (int)Math.Round(avatar.ContentLength / 1024.0),
                    FileFormat = ext.Replace(".", "").ToLower(),
                    UploadedAt = DateTime.UtcNow
                };
                _repository.CreateProfilePhoto(photo);
                _repository.SaveChanges();

                // Link to Account
                account.PhotoID = photo.PhotoID;

                return new UploadResult { Success = true };
            }
            catch (Exception ex)
            {
                return new UploadResult
                {
                    Success = false,
                    ErrorMessage = $"Lỗi khi upload avatar: {ex.Message}"
                };
            }
        }

        private class UploadResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
