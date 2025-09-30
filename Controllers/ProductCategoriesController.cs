using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;


namespace Sistema.Controllers
{
    public class ProductsCategoriesController : Controller
    {
        private readonly IProductCategoryRepository _categoryRepository;

        public ProductsCategoriesController(IProductCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: CategoriasProdutos
        public IActionResult Index()
        {
            var categorias = _categoryRepository.GetAll().OrderBy(c => c.Name);
            return View(categorias);

        }

        // GET: CategoriasProdutos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
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
        public async Task<IActionResult> Create(ProductCategory category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.CreateAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: CategoriasProdutos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: CategoriasProdutos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductCategory category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _categoryRepository.UpdateAsync(category);
                }
                catch
                {
                    if (!await _categoryRepository.ExistsAsync(category.ProductCategoryId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: CategoriasProdutos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: CategoriasProdutos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            await _categoryRepository.DeleteAsync(category);
            return RedirectToAction(nameof(Index));
        }        
    }
}
