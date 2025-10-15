using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminSuppliersController : Controller
    {
       
        private readonly ISupplierRepository _supplierRepository;

        public AdminSuppliersController(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }
        //INDEX
        // GET: Fornecedores
        public IActionResult Index()
        {
            var suppliers = _supplierRepository.GetAll();
            return View(suppliers);
        }
        // Details
        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _supplierRepository.GetByIdWithProductsAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // GET: Fornecedores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Fornecedores/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            Console.WriteLine("=== INÍCIO DO MÉTODO CREATE SUPPLIER (POST) ===");
            Console.WriteLine($"Supplier recebido - Nome: {supplier.Name}, Telefone: {supplier.Phone}");

            if (ModelState.IsValid)
            {
                try
                {
                    await _supplierRepository.CreateAsync(supplier);
                    Console.WriteLine("Fornecedor salvo com sucesso no banco de dados!");
                    TempData["SuccessMessage"] = "Fornecedor criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRO ao criar fornecedor: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Erro ao criar fornecedor: {ex.Message}";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                Console.WriteLine($"Erro de validação: {errorMessage}");
                TempData["ErrorMessage"] = errorMessage;
            }
            
            return View(supplier);
        }

        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _supplierRepository.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Fornecedores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.SupplierId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    await _supplierRepository.UpdateAsync(supplier);
                    TempData["SuccessMessage"] = "Fornecedor atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRO ao atualizar fornecedor: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Erro ao atualizar fornecedor: {ex.Message}";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                Console.WriteLine($"Erro de validação: {errorMessage}");
                TempData["ErrorMessage"] = errorMessage;
            }
            
            return View(supplier);
        }

        // GET: Fornecedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var supplier = await _supplierRepository.GetByIdAsync(id.Value);
            if (supplier == null) return NotFound();

            return View(supplier);
        }

        // POST: Fornecedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var supplier = await _supplierRepository.GetByIdAsync(id);
                if (supplier != null)
                {
                    await _supplierRepository.DeleteAsync(supplier);
                    TempData["SuccessMessage"] = "Fornecedor excluído com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Fornecedor não encontrado.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO ao excluir fornecedor: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                TempData["ErrorMessage"] = $"Erro ao excluir fornecedor: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }
        
    }
}
