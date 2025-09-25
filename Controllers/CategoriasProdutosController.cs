using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Controllers
{
    public class CategoriasProdutosController : Controller
    {
        private readonly SistemaDbContext _context;

        public CategoriasProdutosController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: CategoriasProdutos
        public async Task<IActionResult> Index()
        {
            return View(await _context.CategoriasProdutos.ToListAsync());
        }

        // GET: CategoriasProdutos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaProduto = await _context.CategoriasProdutos
                .FirstOrDefaultAsync(m => m.CategoriaProdutoId == id);
            if (categoriaProduto == null)
            {
                return NotFound();
            }

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
        public async Task<IActionResult> Create([Bind("CategoriaProdutoId,Nome")] CategoriaProduto categoriaProduto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categoriaProduto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoriaProduto);
        }

        // GET: CategoriasProdutos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaProduto = await _context.CategoriasProdutos.FindAsync(id);
            if (categoriaProduto == null)
            {
                return NotFound();
            }
            return View(categoriaProduto);
        }

        // POST: CategoriasProdutos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaProdutoId,Nome")] CategoriaProduto categoriaProduto)
        {
            if (id != categoriaProduto.CategoriaProdutoId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categoriaProduto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaProdutoExists(categoriaProduto.CategoriaProdutoId))
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
            return View(categoriaProduto);
        }

        // GET: CategoriasProdutos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoriaProduto = await _context.CategoriasProdutos
                .FirstOrDefaultAsync(m => m.CategoriaProdutoId == id);
            if (categoriaProduto == null)
            {
                return NotFound();
            }

            return View(categoriaProduto);
        }

        // POST: CategoriasProdutos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoriaProduto = await _context.CategoriasProdutos.FindAsync(id);
            if (categoriaProduto != null)
            {
                _context.CategoriasProdutos.Remove(categoriaProduto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaProdutoExists(int id)
        {
            return _context.CategoriasProdutos.Any(e => e.CategoriaProdutoId == id);
        }
    }
}
