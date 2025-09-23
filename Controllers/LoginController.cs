using Microsoft.AspNetCore.Mvc;

namespace Sistema.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Views/Login/Index.cshtml
        }

        public IActionResult RecuperarSenha()
        {
            return View(); // Views/Login/RecuperarSenha.cshtml
        }
    }
}