using Microsoft.AspNetCore.Mvc;
using Orneholm.CognitiveJukebox.Web.Models;

namespace Orneholm.CognitiveJukebox.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult<HomeIndexViewModel> Index()
        {
            return View();
        }
    }
}
