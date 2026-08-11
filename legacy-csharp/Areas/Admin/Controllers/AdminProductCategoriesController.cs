using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Repository.Interfaces;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductCategoriesController : Controller
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public AdminProductCategoriesController(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        // GET: Admin/ProductCategories
        public async Task<IActionResult> Index()
        {
            var categories = _productCategoryRepository.GetAll()
                .OrderBy(c => c.Name);
            
            return View(await categories.ToListAsync());
        }

        // GET: Admin/ProductCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _productCategoryRepository.GetByIdAsync(id.Value);
            
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // GET: Admin/ProductCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ProductCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductCategoryId,Name")] ProductCategory productCategory)
        {
            Console.WriteLine("=== INÍCIO DO MÉTODO CREATE CATEGORY (POST) ===");
            Console.WriteLine($"Category recebida - Nome: {productCategory.Name}");

            if (ModelState.IsValid)
            {
                try
                {
                    await _productCategoryRepository.CreateAsync(productCategory);
                    Console.WriteLine("Categoria salva com sucesso no banco de dados!");
                    TempData["SuccessMessage"] = "Categoria de produto criada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRO ao criar categoria: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Erro ao criar categoria: {ex.Message}";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                Console.WriteLine($"Erro de validação: {errorMessage}");
                TempData["ErrorMessage"] = errorMessage;
            }
            return View(productCategory);
        }

        // GET: Admin/ProductCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _productCategoryRepository.GetByIdAsync(id.Value);
            if (productCategory == null)
            {
                return NotFound();
            }
            return View(productCategory);
        }

        // POST: Admin/ProductCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductCategoryId,Name")] ProductCategory productCategory)
        {
            if (id != productCategory.ProductCategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productCategoryRepository.UpdateAsync(productCategory);
                    
                    TempData["SuccessMessage"] = "Categoria de produto atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProductCategoryExists(productCategory.ProductCategoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productCategory);
        }

        // GET: Admin/ProductCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _productCategoryRepository.GetByIdAsync(id.Value);
            if (productCategory == null)
            {
                return NotFound();
            }

            return View(productCategory);
        }

        // POST: Admin/ProductCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productCategory = await _productCategoryRepository.GetByIdAsync(id);
            if (productCategory != null)
            {
                await _productCategoryRepository.DeleteAsync(productCategory);
                
                TempData["SuccessMessage"] = "Categoria de produto excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProductCategoryExists(int id)
        {
            return await _productCategoryRepository.ExistsAsync(id);
        }
    }
}