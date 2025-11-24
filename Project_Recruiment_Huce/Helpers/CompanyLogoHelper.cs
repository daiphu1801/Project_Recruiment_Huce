using System;
using System.Linq;
using System.Configuration;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để lấy URL logo công ty
    /// Tập trung logic để tránh trùng lặp giữa các controllers
    /// </summary>
    public static class CompanyLogoHelper
    {
        private const string DefaultLogoUrl = "/Content/images/job_logo_1.jpg";

        /// <summary>
        /// Lấy URL logo công ty từ tin tuyển dụng
        /// Ưu tiên lấy từ Company, nếu không có thì lấy từ Recruiter's Company
        /// </summary>
        /// <param name="job">Entity JobPost</param>
        /// <returns>URL logo hoặc logo mặc định nếu không tìm thấy</returns>
        public static string GetLogoUrl(JobPost job)
        {
            // Thử lấy logo từ Company trước
            if (job?.Company?.PhotoID != null && job.Company.PhotoID.HasValue)
            {
                return GetLogoUrlByPhotoId(job.Company.PhotoID.Value);
            }

            // Nếu không có Company, thử lấy từ Company của Recruiter
            if (job?.Recruiter?.Company?.PhotoID != null && job.Recruiter.Company.PhotoID.HasValue)
            {
                return GetLogoUrlByPhotoId(job.Recruiter.Company.PhotoID.Value);
            }

            return DefaultLogoUrl;
        }

        /// <summary>
        /// Lấy URL logo công ty từ entity Company
        /// </summary>
        /// <param name="company">Entity Company</param>
        /// <returns>URL logo hoặc logo mặc định nếu không có</returns>
        public static string GetLogoUrl(Company company)
        {
            if (company?.PhotoID == null || !company.PhotoID.HasValue)
            {
                return DefaultLogoUrl;
            }

            return GetLogoUrlByPhotoId(company.PhotoID.Value);
        }

        /// <summary>
        /// Lấy URL logo công ty từ PhotoID
        /// Tạo database context mới để truy vấn
        /// </summary>
        /// <param name="photoId">ID của ảnh</param>
        /// <returns>URL logo hoặc logo mặc định nếu không tìm thấy</returns>
        public static string GetLogoUrlByPhotoId(int photoId)
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
                using (var db = new JOBPORTAL_ENDataContext(connectionString))
                {
                    db.ObjectTrackingEnabled = false; // Tắt tracking vì chỉ đọc
                    var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId);
                    if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                    {
                        return photo.FilePath;
                    }
                }
            }
            catch (Exception)
            {
                // Nếu có lỗi, trả về logo mặc định
                return DefaultLogoUrl;
            }

            return DefaultLogoUrl;
        }

        /// <summary>
        /// Lấy URL logo công ty với database context đã có sẵn (hiệu quả hơn)
        /// Sử dụng khi đã có context để tránh tạo connection mới
        /// </summary>
        /// <param name="db">Database context hiện tại</param>
        /// <param name="photoId">ID của ảnh</param>
        /// <returns>URL logo hoặc logo mặc định nếu không tìm thấy</returns>
        public static string GetLogoUrlWithContext(JOBPORTAL_ENDataContext db, int? photoId)
        {
            if (!photoId.HasValue)
            {
                return DefaultLogoUrl;
            }

            try
            {
                var photo = db.ProfilePhotos.FirstOrDefault(p => p.PhotoID == photoId.Value);
                if (photo != null && !string.IsNullOrEmpty(photo.FilePath))
                {
                    return photo.FilePath;
                }
            }
            catch (Exception)
            {
                return DefaultLogoUrl;
            }

            return DefaultLogoUrl;
        }

        /// <summary>
        /// Lấy URL logo mặc định
        /// </summary>
        /// <returns>URL logo mặc định</returns>
        public static string GetDefaultLogoUrl()
        {
            return DefaultLogoUrl;
        }
    }
}

