using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductCategoriesController : Controller
    {
        private readonly SistemaDbContext _context;

        public ProductCategoriesController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductCategories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.ProductCategories
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            return View(categories);
        }

        // GET: Admin/ProductCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(m => m.ProductCategoryId == id);
            
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
            if (ModelState.IsValid)
            {
                _context.Add(productCategory);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Product category created successfully!";
                return RedirectToAction(nameof(Index));
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

            var productCategory = await _context.ProductCategories.FindAsync(id);
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
                    _context.Update(productCategory);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Product category updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductCategoryExists(productCategory.ProductCategoryId))
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

            var productCategory = await _context.ProductCategories
                .FirstOrDefaultAsync(m => m.ProductCategoryId == id);
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
            var productCategory = await _context.ProductCategories.FindAsync(id);
            if (productCategory != null)
            {
                _context.ProductCategories.Remove(productCategory);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Product category deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductCategoryExists(int id)
        {
            return _context.ProductCategories.Any(e => e.ProductCategoryId == id);
        }
    }
}