using Microsoft.AspNetCore.Mvc;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Controllers
{
    public class FornecedoresController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IFornecedorRepository _fornecedorRepository;

        public FornecedoresController(IFornecedorRepository fornecedorRepository)
        {
            _fornecedorRepository = fornecedorRepository;
        }

        // GET: Fornecedores
        public IActionResult Index()
        {
            var fornecedores = _fornecedorRepository.GetAll();
            return View(fornecedores);
        }

        // GET: Fornecedores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var fornecedor = await _fornecedorRepository.GetByIdWithProdutosAsync(id.Value);
            if (fornecedor == null) return NotFound();

            return View(fornecedor);
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
        public async Task<IActionResult> Create(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                await _fornecedorRepository.CreateAsync(fornecedor);
                return RedirectToAction(nameof(Index));
            }
            return View(fornecedor);
        }

        // GET: Fornecedores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var fornecedor = await _fornecedorRepository.GetByIdAsync(id.Value);
            if (fornecedor == null) return NotFound();

            return View(fornecedor);
        }

        // POST: Fornecedores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Fornecedor fornecedor)
        {
            if (id != fornecedor.FornecedorId) return NotFound();

            if (ModelState.IsValid)
            {
                await _fornecedorRepository.UpdateAsync(fornecedor);
                return RedirectToAction(nameof(Index));
            }
            return View(fornecedor);
        }

        // GET: Fornecedores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var fornecedor = await _fornecedorRepository.GetByIdAsync(id.Value);
            if (fornecedor == null) return NotFound();

            return View(fornecedor);
        }

        // POST: Fornecedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var fornecedor = await _fornecedorRepository.GetByIdAsync(id);
            await _fornecedorRepository.DeleteAsync(fornecedor);
            return RedirectToAction(nameof(Index));
        }
        
    }
}
