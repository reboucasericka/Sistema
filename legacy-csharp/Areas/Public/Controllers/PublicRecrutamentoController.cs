using Microsoft.AspNetCore.Mvc;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicRecrutamentoController : Controller
    {
        // GET: /Recrutamento/
        public IActionResult Index()
        {
            return View();
        }
    }
}
