using Microsoft.AspNetCore.Mvc;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class RecrutamentoController : Controller
    {
        // GET: /Recrutamento/
        public IActionResult Index()
        {
            return View();
        }
    }
}
