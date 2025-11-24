using System;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin.Security;
using Microsoft.Owin;

namespace Project_Recruiment_Huce.Areas.Admin.Filters
{
    /// <summary>
    /// Filter toàn cục cho Admin area để đảm bảo tất cả các request đều được kiểm tra authentication
    /// Áp dụng cho tất cả controllers trong Admin area (trừ AuthController)
    /// </summary>
    public class AdminAreaAuthorizationFilter : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // Bỏ qua filter cho AuthController
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            if (controllerName.Equals("Auth", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Kiểm tra xem request có trong Admin area không
            if (filterContext.RouteData.DataTokens.ContainsKey("area") &&
                filterContext.RouteData.DataTokens["area"]?.ToString() == "Admin")
            {
                // Kiểm tra authentication với AdminCookie cụ thể
                var owinContext = filterContext.HttpContext.GetOwinContext();
                var authenticationResult = owinContext.Authentication.AuthenticateAsync("AdminCookie").Result;
                
                if (authenticationResult == null || !authenticationResult.Identity.IsAuthenticated)
                {
                    // Chưa đăng nhập với AdminCookie - redirect đến Login
                    var returnUrl = filterContext.HttpContext.Request.RawUrl;
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new { controller = "Auth", action = "Login", area = "Admin", returnUrl = returnUrl }
                        )
                    );
                    return;
                }

                // Kiểm tra role Admin
                var identity = authenticationResult.Identity as ClaimsIdentity;
                if (identity != null)
                {
                    var roleClaim = identity.FindFirst(ClaimTypes.Role);
                    if (roleClaim == null || roleClaim.Value != "Admin")
                    {
                        // Đã đăng nhập nhưng không có quyền Admin
                        filterContext.Result = new RedirectToRouteResult(
                            new System.Web.Routing.RouteValueDictionary(
                                new { controller = "Auth", action = "Login", area = "Admin" }
                            )
                        );
                        return;
                    }

                    // IMPORTANT: Set context.User to AdminCookie identity for AntiForgeryToken compatibility
                    filterContext.HttpContext.User = new ClaimsPrincipal(identity);
                }
                else
                {
                    // Không có identity - redirect đến Login
                    var returnUrl = filterContext.HttpContext.Request.RawUrl;
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new { controller = "Auth", action = "Login", area = "Admin", returnUrl = returnUrl }
                        )
                    );
                }
            }
        }
    }
}

