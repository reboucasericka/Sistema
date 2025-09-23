using Microsoft.AspNetCore.Mvc;

namespace Sistema.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult NotFoundPage()
        {
            return View("NotFound");
        }

        public IActionResult ServerError()
        {
            return View("ServerError");
        }
    }
}
