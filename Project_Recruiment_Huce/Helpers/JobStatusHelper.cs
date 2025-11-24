using System;
using System.Linq;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để quản lý và chuẩn hóa trạng thái của tin tuyển dụng
    /// </summary>
    public static class JobStatusHelper
    {
        // Các trạng thái chuẩn của tin tuyển dụng
        public const string Published = "Published"; // Đã đăng - Hiển thị công khai
        public const string Closed = "Closed";       // Đã đóng - Không nhận ứng tuyển nữa
        public const string Hidden = "Hidden";       // Ẩn - Không hiển thị
        public const string Draft = "Draft";         // Bản nháp - Chưa đăng

        /// <summary>
        /// Chuẩn hóa các trạng thái cũ (ví dụ: Visible) thành trạng thái hiện tại (Published)
        /// Thực hiện cập nhật trực tiếp trong database để đảm bảo tính nhất quán
        /// </summary>
        /// <param name="db">Database context</param>
        public static void NormalizeStatuses(JOBPORTAL_ENDataContext db)
        {
            if (db == null) return;

            if (db.JobPosts.Any(j => j.Status == "Visible"))
            {
                db.ExecuteCommand("UPDATE [dbo].[JobPosts] SET [Status] = {0} WHERE [Status] = {1}",
                    Published, "Visible");
            }
        }

        /// <summary>
        /// Trả về trạng thái đã chuẩn hóa mà không lưu vào database
        /// Sử dụng cho việc hiển thị hoặc so sánh trạng thái
        /// </summary>
        /// <param name="status">Trạng thái cần chuẩn hóa</param>
        /// <returns>Trạng thái đã chuẩn hóa</returns>
        public static string NormalizeStatus(string status)
        {
            if (string.Equals(status, "Visible", StringComparison.OrdinalIgnoreCase))
            {
                return Published;
            }
            return status;
        }

        /// <summary>
        /// Kiểm tra xem tin tuyển dụng có đang ở trạng thái đã đăng hay không
        /// </summary>
        /// <param name="status">Trạng thái cần kiểm tra</param>
        /// <returns>True nếu đang Published, false nếu không</returns>
        public static bool IsPublished(string status)
        {
            return string.Equals(status, Published, StringComparison.OrdinalIgnoreCase);
        }
    }
}

