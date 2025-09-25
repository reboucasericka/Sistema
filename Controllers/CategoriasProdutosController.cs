using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;


namespace Sistema.Controllers
{
    public class CategoriasProdutosController : Controller
    {
        private readonly ICategoriaProdutoRepository _categoriaProdutoRepository;

        public CategoriasProdutosController(ICategoriaProdutoRepository categoriaProdutoRepository)
        {
            _categoriaProdutoRepository = categoriaProdutoRepository;
        }

        // GET: CategoriasProdutos
        public IActionResult Index()
        {
            var categorias = _categoriaProdutoRepository.GetAll().OrderBy(c => c.Nome);
            return View(categorias);

        }

        // GET: CategoriasProdutos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var categoriaProduto = await _categoriaProdutoRepository.GetByIdAsync(id.Value);
            if (categoriaProduto == null) return NotFound();

            return View(categoriaProduto);
        }

        // GET: CategoriasProdutos/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CategoriasProdutos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaProduto categoriaProduto)
        {
            if (ModelState.IsValid)
            {
                await _categoriaProdutoRepository.CreateAsync(categoriaProduto);
                return RedirectToAction(nameof(Index));
            }
            return View(categoriaProduto);
        }

        // GET: CategoriasProdutos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _categoriaProdutoRepository.GetByIdAsync(id.Value);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: CategoriasProdutos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoriaProduto categoriaProduto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoriaProdutoRepository.UpdateAsync(categoriaProduto);
                }
                catch
                {
                    if (!await _categoriaProdutoRepository.ExistsAsync(categoriaProduto.CategoriaProdutoId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categoriaProduto);
        }

        // GET: CategoriasProdutos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var categoria = await _categoriaProdutoRepository.GetByIdAsync(id.Value);
            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: CategoriasProdutos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _categoriaProdutoRepository.GetByIdAsync(id);
            await _categoriaProdutoRepository.DeleteAsync(categoria);
            return RedirectToAction(nameof(Index));
        }        
    }
}
