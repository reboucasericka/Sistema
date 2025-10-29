using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sistema.Services.Api;
using SistemaAPI.DTOs;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductCategoriesController : Controller
    {
        private readonly IApiProductCategoriesService _apiService;
        private readonly ILogger<AdminProductCategoriesController> _logger;

        public AdminProductCategoriesController(IApiProductCategoriesService apiService, ILogger<AdminProductCategoriesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/ProductCategories
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiService.GetAllAsync();
                
                if (response.IsSuccess && response.Data != null)
                {
                    var categories = response.Data.OrderBy(c => c.Name).ToList();
                    return View(categories);
                }
                else
                {
                    _logger.LogError("Failed to fetch product categories: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Failed to load product categories. Please try again.";
                    return View(new List<ProductCategoryDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product categories");
                TempData["ErrorMessage"] = "An error occurred while loading product categories.";
                return View(new List<ProductCategoryDto>());
            }
        }

        // GET: Admin/ProductCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _apiService.GetByIdAsync(id.Value);
                
                if (!response.IsSuccess || response.Data == null)
                {
                    return NotFound();
                }

                return View(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product category details for ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading product category details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/ProductCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/ProductCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCategoryDto category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _apiService.CreateAsync(category);
                    
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Categoria de produto criada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to create product category: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Erro ao criar categoria: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product category");
                    TempData["ErrorMessage"] = "Erro ao criar categoria. Tente novamente.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                TempData["ErrorMessage"] = errorMessage;
            }
            
            return View(category);
        }

        // GET: Admin/ProductCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _apiService.GetByIdAsync(id.Value);
                
                if (!response.IsSuccess || response.Data == null)
                {
                    return NotFound();
                }

                return View(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product category for edit with ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading product category for edit.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/ProductCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCategoryDto category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _apiService.UpdateAsync(id, category);
                    
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Categoria de produto atualizada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update product category: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Erro ao atualizar categoria: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product category with ID {Id}", id);
                    TempData["ErrorMessage"] = "Erro ao atualizar categoria. Tente novamente.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                TempData["ErrorMessage"] = errorMessage;
            }
            
            return View(category);
        }

        // GET: Admin/ProductCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _apiService.GetByIdAsync(id.Value);
                
                if (!response.IsSuccess || response.Data == null)
                {
                    return NotFound();
                }

                return View(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product category for delete with ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading product category for delete.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/ProductCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync(id);
                
                if (response.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Categoria de produto excluída com sucesso!";
                }
                else
                {
                    _logger.LogError("Failed to delete product category: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Erro ao excluir categoria: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product category with ID {Id}", id);
                TempData["ErrorMessage"] = "Erro ao excluir categoria. Tente novamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}