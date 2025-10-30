using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicPriceController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicPriceController(SistemaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            // Limpar o ChangeTracker para forçar atualização
            _context.ChangeTracker.Clear();

            var price = await _context.PriceTables
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PriceId == id);
            
            if (price == null) return NotFound();

            return View(price);
        }
    }
}