using System.Web.Mvc;

namespace Project_Recruiment_Huce.Controllers
{
    public class JobsController : Controller
    {
        public ActionResult JobsListing()
        {
            return View();
        }

        public ActionResult JobsDetails(int? id)
        {
            ViewBag.JobId = id;
            return View();
        }
    }
}


