using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicProductsController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicProductsController(SistemaDbContext context)
        {
            _context = context;
        }

        // Lista pública
        public async Task<IActionResult> Index()
        {
            // Limpar o ChangeTracker para forçar atualização
            _context.ChangeTracker.Clear();

            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductCategory.Name)
                .ThenBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // Detalhes público
        public async Task<IActionResult> Details(int id)
        {
            // Limpar o ChangeTracker para forçar atualização
            _context.ChangeTracker.Clear();

            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            
            if (product == null)
            {
                TempData["Error"] = "Produto não encontrado.";
                return RedirectToAction("Index");
            }

            return View(product);
        }
    }
}