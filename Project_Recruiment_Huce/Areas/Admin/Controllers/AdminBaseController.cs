using System.Web.Mvc;
using Project_Recruiment_Huce.Areas.Admin.Attributes;

namespace Project_Recruiment_Huce.Areas.Admin.Controllers
{
    /// <summary>
    /// Base controller cho tất cả controllers trong Admin area
    /// Tự động áp dụng AdminAuthorize để yêu cầu đăng nhập và quyền Admin
    /// </summary>
    [AdminAuthorize]
    public abstract class AdminBaseController : Controller
    {
        // Tất cả controllers kế thừa từ AdminBaseController sẽ tự động được bảo vệ bởi AdminAuthorize
    }
}

