using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly IProdutoRepository _repository;

        public ProdutosController(IProdutoRepository repository )
        {
            _repository = repository;
        }

        // GET: Produtos
        public IActionResult Index()
        {
            var produtos = _repository.GetAllProducts(); // já vem com Categoria e Fornecedor
            return View(_repository.GetAllProducts());
        }

        // GET: Produtos/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();

            var produto = _repository.GetProductById(id.Value, incluirRelacionamentos: true);
            if (produto == null) return NotFound();

            return View(produto);
        }

        // GET: Produtos/Create
        public IActionResult Create()
        {
            ViewData["CategoriaProdutoId"] = new SelectList(_repository.GetAllCategoriasProdutos(), "CategoriaProdutoId", "Nome");
            ViewData["FornecedorId"] = new SelectList(_repository.GetAllFornecedores(), "FornecedorId", "Nome");
            return View();
        }

        // POST: Produtos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Produto produto)
        {
            if (ModelState.IsValid)
            {
                _repository.AddProduct(produto);
                await _repository.SaveAllAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaProdutoId"] = new SelectList(_repository.GetAllCategoriasProdutos(), "CategoriaProdutoId", "Nome", produto.CategoriaProdutoId);
            ViewData["FornecedorId"] = new SelectList(_repository.GetAllFornecedores(), "FornecedorId", "Nome", produto.FornecedorId);
            return View(produto);
        }

        // GET: Produtos/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return NotFound();

            var produto = _repository.GetProductById(id.Value);
            if (produto == null) return NotFound();

            ViewData["CategoriaProdutoId"] = new SelectList(_repository.GetAllCategoriasProdutos(), "CategoriaProdutoId", "Nome", produto.CategoriaProdutoId);
            ViewData["FornecedorId"] = new SelectList(_repository.GetAllFornecedores(), "FornecedorId", "Nome", produto.FornecedorId);
            return View(produto);
        }

        // POST: Produtos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Produto produto)
        {
            if (id != produto.ProdutoId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _repository.UpdateProduct(produto);
                    await _repository.SaveAllAsync();
                }
                catch
                {
                    if (!_repository.ProdutoExists(produto.ProdutoId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaProdutoId"] = new SelectList(_repository.GetAllCategoriasProdutos(), "CategoriaProdutoId", "Nome", produto.CategoriaProdutoId);
            ViewData["FornecedorId"] = new SelectList(_repository.GetAllFornecedores(), "FornecedorId", "Nome", produto.FornecedorId);
            return View(produto);
        }

        // GET: Produtos/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return NotFound();

            var produto = _repository.GetProductById(id.Value, incluirRelacionamentos: true);
            if (produto == null) return NotFound();

            return View(produto);
        }

        // POST: Produtos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = _repository.GetProductById(id);
            _repository.RemoveProduct(produto);
            await _repository.SaveAllAsync();
            return RedirectToAction(nameof(Index));
        }
        
    }
}
