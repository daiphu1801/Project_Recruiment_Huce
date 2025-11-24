using System;
using System.Configuration;
using Project_Recruiment_Huce.Models;

namespace Project_Recruiment_Huce.Infrastructure
{
    /// <summary>
    /// Factory class để tạo database context instances
    /// Tập trung quản lý connection string và giảm code trùng lặp
    /// </summary>
    public static class DbContextFactory
    {
        private static readonly string ConnectionString;

        static DbContextFactory()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"]?.ConnectionString;
            
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new InvalidOperationException("Không tìm thấy JOBPORTAL_ENConnectionString trong configuration");
            }
        }

        /// <summary>
        /// Tạo database context instance mới cho các thao tác ghi (Insert/Update/Delete)
        /// Object tracking được bật để LINQ to SQL theo dõi thay đổi
        /// </summary>
        /// <returns>Instance mới của JOBPORTAL_ENDataContext với tracking enabled</returns>
        public static JOBPORTAL_ENDataContext Create()
        {
             var db = new JOBPORTAL_ENDataContext(ConnectionString);

            // Bật tracking cho các thao tác ghi dữ liệu
            db.ObjectTrackingEnabled = true;
            db.DeferredLoadingEnabled = true; // Bật lazy loading cho navigation properties

            return db;
        }

        /// <summary>
        /// Tạo database context instance mới cho các thao tác chỉ đọc
        /// Object tracking được tắt để tăng hiệu suất cho các query không cần thay đổi dữ liệu
        /// </summary>
        /// <returns>Instance mới của JOBPORTAL_ENDataContext với tracking disabled</returns>
        public static JOBPORTAL_ENDataContext CreateReadOnly()
        {
            var db = new JOBPORTAL_ENDataContext(ConnectionString);
            db.ObjectTrackingEnabled = false; // Tắt tracking để tối ưu hiệu suất
            return db;
        }

        /// <summary>
        /// Lấy connection string từ configuration
        /// </summary>
        /// <returns>Connection string đến database</returns>
        public static string GetConnectionString()
        {
            return ConnectionString;
        }
    }
}

