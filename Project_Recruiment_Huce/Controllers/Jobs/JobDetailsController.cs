using System.Web.Mvc;

namespace Project_Recruiment_Huce.Controllers
{
    public class JobDetailsController : Controller
    {
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("JobsListing", "Jobs");
            }

            return RedirectToAction("JobDetails", "Jobs", new { id = id.Value });
        }
    }
}

