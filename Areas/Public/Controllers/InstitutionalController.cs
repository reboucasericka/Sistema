using Microsoft.AspNetCore.Mvc;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class InstitutionalController : Controller
    {
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Academy()
        {
            return View();
        }
    }
}
