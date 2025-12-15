using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Helpers;
using System.Security.Claims;

[assembly: OwinStartup(typeof(Project_Recruiment_Huce.Startup))]
namespace Project_Recruiment_Huce
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cấu hình Hangfire - PHẢI được cấu hình đầu tiên
            var connectionString = ConfigurationManager.ConnectionStrings["JOBPORTAL_ENConnectionString"].ConnectionString;
            
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });

            // Khởi động Hangfire Server
            app.UseHangfireServer();
            
            // Tùy chọn: Thêm Dashboard (truy cập tại /hangfire)
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() }
            });

            // CRITICAL: External cookie PHẢI được cấu hình TRƯỚC TẤT CẢ các cookie khác
            app.UseExternalSignInCookie(Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie);
            
            // Cookie cho User
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "UserCookie",
                LoginPath = new PathString("/Account/Login"),
                CookieName = "UserAuth",
                ExpireTimeSpan = System.TimeSpan.FromHours(2),
                SlidingExpiration = true,
                CookieSecure = CookieSecureOption.SameAsRequest,
                CookieHttpOnly = true,
                AuthenticationMode = AuthenticationMode.Active // Active cho User area
            });

            // Cookie cho Admin
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "AdminCookie",
                LoginPath = new PathString("/Admin/Auth/Login"),
                CookieName = "AdminAuth",
                ExpireTimeSpan = System.TimeSpan.FromHours(2),
                SlidingExpiration = true,
                CookieSecure = CookieSecureOption.SameAsRequest,
                CookieHttpOnly = true,
                CookiePath = "/Admin", // Quan trọng: Cookie chỉ áp dụng cho Admin area
                AuthenticationMode = AuthenticationMode.Active // Active cho Admin area
            });

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            
            // Google OAuth Configuration
            var clientId = ConfigurationManager.AppSettings["GoogleClientId"];
            var clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            
            var googleOptions = new GoogleOAuth2AuthenticationOptions
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                // KHÔNG set CallbackPath - để OWIN tự động xử lý với ExternalLoginCallback
                SignInAsAuthenticationType = Microsoft.AspNet.Identity.DefaultAuthenticationTypes.ExternalCookie,
                Provider = new GoogleOAuth2AuthenticationProvider
                {
                    OnAuthenticated = context =>
                    {
                        // Thêm avatar URL từ Google vào claims
                        var pictureUrl = context.User.SelectToken("picture")?.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(pictureUrl))
                        {
                            context.Identity.AddClaim(new Claim("picture", pictureUrl));
                        }
                        return System.Threading.Tasks.Task.FromResult(0);
                    }
                }
            };
            
            // Thêm scope cần thiết
            googleOptions.Scope.Add("email");
            googleOptions.Scope.Add("profile");
            
            app.UseGoogleAuthentication(googleOptions);
        }
    }
}