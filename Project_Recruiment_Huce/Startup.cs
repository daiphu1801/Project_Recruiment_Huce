using Microsoft.Owin;
using Owin;
using System.Web.Helpers;
using System.Security.Claims;

[assembly: OwinStartupAttribute(typeof(Project_Recruiment_Huce.Startup))]
namespace Project_Recruiment_Huce
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Configure anti-forgery token to use NameIdentifier claim
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            
            ConfigureAuth(app);
        }
    }
}
