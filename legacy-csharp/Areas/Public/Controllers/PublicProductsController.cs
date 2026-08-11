using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicProductsController : Controller
    {
        private readonly IProductRepository _productRepository;

        public PublicProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Lista pública
        public IActionResult Index()
        {
            var product = _productRepository
                .GetAllWithIncludes()
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductCategory.Name)
                .ThenBy(p => p.Name)
                .ToList();

            return View(product);
        }

        // Detalhes público
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdWithIncludesAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Produto não encontrado.";
                return RedirectToAction("Index");
            }

            return View(product);
        }
    }
}