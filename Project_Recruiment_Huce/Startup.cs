using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using System.Configuration;
using System.Web.Helpers;
using System.Security.Claims;

[assembly: OwinStartup(typeof(Project_Recruiment_Huce.Startup))]
namespace Project_Recruiment_Huce
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cookie cho User
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "UserCookie",
                LoginPath = new PathString("/Account/Login"),
                CookieName = "UserAuth",
                ExpireTimeSpan = System.TimeSpan.FromHours(2),
                SlidingExpiration = true,
                CookieSecure = CookieSecureOption.SameAsRequest // Thay Always bằng SameAsRequest cho dev
            });

            // Cookie cho Admin
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "AdminCookie",
                LoginPath = new PathString("/Admin/Auth/Login"),
                CookieName = "AdminAuth",
                ExpireTimeSpan = System.TimeSpan.FromHours(2),
                SlidingExpiration = true,
                CookieSecure = CookieSecureOption.SameAsRequest
            });

            app.SetDefaultSignInAsAuthenticationType("UserCookie");

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            // Google OAuth
            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["GoogleClientId"],
                ClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"],
                CallbackPath = new PathString("/signin-google"),
                SignInAsAuthenticationType = "UserCookie" // Google login cho user
            };
            googleOptions.Scope.Add("email");
            googleOptions.Scope.Add("profile");
            app.UseGoogleAuthentication(googleOptions);
        }
    }
}