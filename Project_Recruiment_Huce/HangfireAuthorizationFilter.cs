using Hangfire.Dashboard;
using System.Web;

namespace Project_Recruiment_Huce
{
    /// <summary>
    /// Authorization filter for Hangfire Dashboard
    /// Only allows access to authenticated admin users
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = HttpContext.Current;
            
            // Cho phép truy cập nếu:
            // 1. Đang trong môi trường development (localhost)
            // 2. Hoặc user đã đăng nhập với AdminCookie
            
            if (httpContext.Request.IsLocal)
            {
                return true; // Cho phép truy cập từ localhost
            }
            
            // Kiểm tra xem user có cookie Admin không
            var adminCookie = httpContext.Request.Cookies["AdminAuth"];
            if (adminCookie != null && !string.IsNullOrEmpty(adminCookie.Value))
            {
                return true;
            }
            
            return false;
        }
    }
}
