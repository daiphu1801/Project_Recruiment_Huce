using System.Security.Claims;
using System.Web;

namespace Project_Recruiment_Huce.Helpers
{
    /// <summary>
    /// Helper class để kiểm tra authentication status cho User và Admin area riêng biệt
    /// </summary>
    public static class AuthHelper
    {
        /// <summary>
        /// Kiểm tra xem user có đăng nhập với UserCookie không (không phải Admin)
        /// </summary>
        public static bool IsUserAuthenticated()
        {
            if (HttpContext.Current == null) return false;
            
            var owinContext = HttpContext.Current.GetOwinContext();
            var authResult = owinContext.Authentication.AuthenticateAsync("UserCookie").Result;
            
            return authResult != null && authResult.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Kiểm tra xem admin có đăng nhập với AdminCookie không
        /// </summary>
        public static bool IsAdminAuthenticated()
        {
            if (HttpContext.Current == null) return false;
            
            var owinContext = HttpContext.Current.GetOwinContext();
            var authResult = owinContext.Authentication.AuthenticateAsync("AdminCookie").Result;
            
            return authResult != null && authResult.Identity.IsAuthenticated;
        }

        /// <summary>
        /// Lấy ClaimsIdentity từ UserCookie
        /// </summary>
        public static ClaimsIdentity GetUserIdentity()
        {
            if (HttpContext.Current == null) return null;
            
            var owinContext = HttpContext.Current.GetOwinContext();
            var authResult = owinContext.Authentication.AuthenticateAsync("UserCookie").Result;
            
            return authResult?.Identity as ClaimsIdentity;
        }

        /// <summary>
        /// Lấy ClaimsIdentity từ AdminCookie
        /// </summary>
        public static ClaimsIdentity GetAdminIdentity()
        {
            if (HttpContext.Current == null) return null;
            
            var owinContext = HttpContext.Current.GetOwinContext();
            var authResult = owinContext.Authentication.AuthenticateAsync("AdminCookie").Result;
            
            return authResult?.Identity as ClaimsIdentity;
        }

        /// <summary>
        /// Lấy role claim từ UserCookie
        /// </summary>
        public static string GetUserRole()
        {
            var identity = GetUserIdentity();
            return identity?.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Lấy username từ UserCookie
        /// </summary>
        public static string GetUserName()
        {
            var identity = GetUserIdentity();
            return identity?.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}
