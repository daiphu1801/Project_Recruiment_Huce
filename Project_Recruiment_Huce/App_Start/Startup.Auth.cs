using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace Project_Recruiment_Huce
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure User Authentication Cookie (for main site users) - ACTIVE to set context.User
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "UserCookie",
                LoginPath = new PathString("/Account/Login"),
                CookieName = "UserAuth",
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromHours(24),
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                CookieSecure = CookieSecureOption.SameAsRequest
            });

            // Configure Admin Authentication Cookie (for admin panel) - PASSIVE since UserCookie is active
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "AdminCookie",
                LoginPath = new PathString("/Admin/Auth/Login"),
                CookieName = "AdminAuth",
                SlidingExpiration = true,
                ExpireTimeSpan = TimeSpan.FromHours(8),
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Passive
            });            
        }
    }
}