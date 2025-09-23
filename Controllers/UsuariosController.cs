using Microsoft.AspNetCore.Mvc;

namespace Sistema.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
