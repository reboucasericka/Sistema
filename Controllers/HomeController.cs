using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Models;
using System.Diagnostics;

namespace Sistema.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SistemaDbContext _context;

        public HomeController(ILogger<HomeController> logger, SistemaDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        
        public IActionResult Index()
        {
            return View();
        }
        //  Nova p�gina Store (vitrine de produtos)
        public async Task<IActionResult> Store()
        {
                var products = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.Supplier)
                .ToListAsync();

            return View(products); // procura Views/Home/S
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Admin()
        {
            // Simular dados para o painel administrativo
            ViewBag.TotalClients = 150;
            ViewBag.TotalAppointments = 12;
            ViewBag.TotalProducts = 45;
            ViewBag.TotalRevenue = "R$ 15.420";
            ViewBag.ActiveClients = 120;
            ViewBag.TotalServices = 8;
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("error/404")]
        public IActionResult Erro404()
        {
            return View();
        }
    }
}
