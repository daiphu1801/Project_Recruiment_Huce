using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.Owin;

namespace Project_Recruiment_Huce.Areas.Admin.Attributes
{
    /// <summary>
    /// Attribute để kiểm tra đăng nhập và quyền Admin trước khi truy cập các trang Admin
    /// Luôn redirect về Login nếu chưa đăng nhập hoặc không có quyền Admin
    /// </summary>
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Kiểm tra authentication với AdminCookie cụ thể
            var owinContext = httpContext.GetOwinContext();
            var authenticationResult = owinContext.Authentication.AuthenticateAsync("AdminCookie").Result;
            
            if (authenticationResult == null || !authenticationResult.Identity.IsAuthenticated)
            {
                return false;
            }

            // Kiểm tra role Admin trong claims
            var identity = authenticationResult.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var roleClaim = identity.FindFirst(ClaimTypes.Role);
                if (roleClaim != null && roleClaim.Value == "Admin")
                {
                    return true;
                }
            }

            return false;
        }
        
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Luôn redirect về Login nếu không authorized
            // Không phân biệt là chưa đăng nhập hay không có quyền
            var returnUrl = filterContext.HttpContext.Request.RawUrl;
            
            // Tránh redirect loop - nếu đang ở trang Login thì không redirect lại
            if (!filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.Equals("Auth", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Auth", action = "Login", area = "Admin", returnUrl = returnUrl }
                    )
                );
            }
            else
            {
                // Nếu đã ở Auth controller, trả về Unauthorized
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}

