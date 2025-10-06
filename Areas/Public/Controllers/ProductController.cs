using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // Lista pública
        public IActionResult Index()
        {
            var produtos = _productRepository
                .GetAllWithIncludes()
                .OrderBy(p => p.ProductCategory.Name)
                .ThenBy(p => p.Name)
                .ToList();

            return View(produtos);
        }

        // Detalhes público
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdWithIncludesAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }
    }
}