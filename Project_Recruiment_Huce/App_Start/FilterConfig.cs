using System.Web;
using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Filters;

namespace Project_Recruiment_Huce
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Đăng ký filter toàn cục cho Admin area
            // Filter này sẽ tự động kiểm tra và chỉ áp dụng cho Admin area
            filters.Add(new AdminAreaAuthorizationFilter());
        }
    }
}
