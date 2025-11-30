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
            
            app.SetDefaultSignInAsAuthenticationType(
                CookieAuthenticationDefaults.AuthenticationType);

           
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Account/Login"),
                CookieSecure = Microsoft.Owin.Security.Cookies.CookieSecureOption.Always
            });

           
            AntiForgeryConfig.UniqueClaimTypeIdentifier =
                ClaimTypes.NameIdentifier;

           
            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["GoogleClientId"],
                ClientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"],
                CallbackPath = new PathString("/signin-google")
            };

            googleOptions.Scope.Add("email");
            googleOptions.Scope.Add("profile");

            app.UseGoogleAuthentication(googleOptions);
        }
    }
}
