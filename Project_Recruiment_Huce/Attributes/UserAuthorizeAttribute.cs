using System;
using System.Web;
using System.Web.Mvc;
using System.Security.Claims;

namespace Project_Recruiment_Huce.Attributes
{
    /// <summary>
    /// Attribute để kiểm tra đăng nhập User (không phải Admin) trước khi truy cập
    /// Chỉ check UserCookie authentication type
    /// </summary>
    public class UserAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Kiểm tra authentication với UserCookie cụ thể
            var owinContext = httpContext.GetOwinContext();
            var authenticationResult = owinContext.Authentication.AuthenticateAsync("UserCookie").Result;
            
            if (authenticationResult == null || !authenticationResult.Identity.IsAuthenticated)
            {
                return false;
            }

            // Kiểm tra role KHÔNG phải Admin
            var identity = authenticationResult.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var roleClaim = identity.FindFirst(ClaimTypes.Role);
                // User phải có role là Candidate hoặc Recruiter (không phải Admin)
                if (roleClaim != null && (roleClaim.Value == "Candidate" || roleClaim.Value == "Recruiter"))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var returnUrl = filterContext.HttpContext.Request.RawUrl;
            
            filterContext.Result = new RedirectToRouteResult(
                new System.Web.Routing.RouteValueDictionary(
                    new { controller = "Account", action = "Login", returnUrl = returnUrl }
                )
            );
        }
    }
}
