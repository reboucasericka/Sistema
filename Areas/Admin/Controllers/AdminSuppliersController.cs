using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sistema.Services.Api;
using SistemaAPI.DTOs;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminSuppliersController : Controller
    {
        private readonly IApiSuppliersService _apiService;
        private readonly ILogger<AdminSuppliersController> _logger;

        public AdminSuppliersController(IApiSuppliersService apiService, ILogger<AdminSuppliersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        //INDEX
        // GET: Fornecedores
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _apiService.GetAllAsync();
                
                if (response.IsSuccess && response.Data != null)
                {
                    var suppliers = response.Data.OrderBy(s => s.Name).ToList();
                    return View(suppliers);
                }
                else
                {
                    _logger.LogError("Failed to fetch suppliers: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Failed to load suppliers. Please try again.";
                    return View(new List<SupplierDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching suppliers");
                TempData["ErrorMessage"] = "An error occurred while loading suppliers.";
                return View(new List<SupplierDto>());
            }
        }

        // Details
        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

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
                _logger.LogError(ex, "Error fetching supplier details for ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading supplier details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Fornecedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fornecedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierDto supplier)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _apiService.CreateAsync(supplier);
                    
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Fornecedor criado com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to create supplier: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Erro ao criar fornecedor: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating supplier");
                    TempData["ErrorMessage"] = "Erro ao criar fornecedor. Tente novamente.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                TempData["ErrorMessage"] = errorMessage;
            }
            
            return View(supplier);
        }

        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

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
                _logger.LogError(ex, "Error fetching supplier for edit with ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading supplier for edit.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Fornecedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierDto supplier)
        {
            if (id != supplier.SupplierId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _apiService.UpdateAsync(id, supplier);
                    
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Fornecedor atualizado com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update supplier: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Erro ao atualizar fornecedor: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating supplier with ID {Id}", id);
                    TempData["ErrorMessage"] = "Erro ao atualizar fornecedor. Tente novamente.";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                TempData["ErrorMessage"] = errorMessage;
            }
            
            return View(supplier);
        }

        // GET: Fornecedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

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
                _logger.LogError(ex, "Error fetching supplier for delete with ID {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading supplier for delete.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Fornecedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync(id);
                
                if (response.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Fornecedor excluído com sucesso!";
                }
                else
                {
                    _logger.LogError("Failed to delete supplier: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Erro ao excluir fornecedor: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID {Id}", id);
                TempData["ErrorMessage"] = "Erro ao excluir fornecedor. Tente novamente.";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}