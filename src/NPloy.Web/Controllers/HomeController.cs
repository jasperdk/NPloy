using System.Web.Mvc;

namespace NPloy.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "NPloy web interface";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About NPloy";

            return View();
        }
    }
}
