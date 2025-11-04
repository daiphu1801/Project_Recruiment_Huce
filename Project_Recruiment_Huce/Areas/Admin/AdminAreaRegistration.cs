using System.Web.Mvc;

namespace Project_Recruiment_Huce.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            // Route for /Admin (root admin URL) - must be registered first
            // This route matches exactly "Admin" with no additional segments
            context.MapRoute(
                "Admin_root",
                "Admin",
                new { controller = "Dashboard", action = "Index" },
                new { },
                namespaces: new[] { "Project_Recruiment_Huce.Areas.Admin.Controllers" }
            );

            // Default admin routes - this will handle /Admin/{controller}/{action}/{id}
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "Project_Recruiment_Huce.Areas.Admin.Controllers" }
            );
        }
    }
}