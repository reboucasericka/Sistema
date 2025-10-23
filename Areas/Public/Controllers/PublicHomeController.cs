using Microsoft.AspNetCore.Mvc;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicHomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
