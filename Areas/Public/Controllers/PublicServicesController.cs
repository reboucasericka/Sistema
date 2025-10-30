using Microsoft.AspNetCore.Mvc;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicServicesController : Controller
    {
        // GET: Public/Services
        public IActionResult Index()
        {
            // Versão estática - sem dependência de DTOs ou API
            return View();
        }
    }
}
