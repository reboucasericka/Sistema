using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StockController : Controller
    {
        private readonly SistemaDbContext _context;

        public StockController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Stock - Lista de produtos com estoque
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.ProductCategory)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            return View(products);
        }

        // GET: Stock/Movements - Histórico de movimentações
        public async Task<IActionResult> Movements()
        {
            var movements = await _context.StockMovements
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.MovementDate)
                .ToListAsync();
            
            return View(movements);
        }

        // GET: Stock/Entry - Entrada de estoque
        public async Task<IActionResult> Entry()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        // POST: Stock/Entry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entry(StockEntry entry)
        {
            if (ModelState.IsValid)
            {
                // Atualizar estoque do produto
                var product = await _context.Products.FindAsync(entry.ProductId);
                if (product != null)
                {
                    product.Stock += entry.Quantity;
                    _context.Update(product);
                }

                // Criar movimentação
                var movement = new StockMovement
                {
                    ProductId = entry.ProductId,
                    MovementDate = DateTime.Now,
                    MovementType = "entry",
                    Quantity = entry.Quantity,
                    Reason = entry.Reason,
                    UserId = User.Identity.Name
                };

                _context.Add(movement);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Entrada de estoque registrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", entry.ProductId);
            return View(entry);
        }

        // GET: Stock/Output - Saída de estoque
        public async Task<IActionResult> Output()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        // POST: Stock/Output
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Output(StockExit output)
        {
            if (ModelState.IsValid)
            {
                // Verificar se há estoque suficiente
                var product = await _context.Products.FindAsync(output.ProductId);
                if (product == null)
                {
                    ModelState.AddModelError("", "Produto não encontrado.");
                    ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", output.ProductId);
                    return View(output);
                }

                if (product.Stock < output.Quantity)
                {
                    ModelState.AddModelError("", $"Estoque insuficiente. Disponível: {product.Stock}");
                    ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", output.ProductId);
                    return View(output);
                }

                // Atualizar estoque do produto
                product.Stock -= output.Quantity;
                _context.Update(product);

                // Criar movimentação
                var movement = new StockMovement
                {
                    ProductId = output.ProductId,
                    MovementDate = DateTime.Now,
                    MovementType = "output",
                    Quantity = output.Quantity,
                    Reason = output.Reason,
                    UserId = User.Identity.Name
                };

                _context.Add(movement);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Saída de estoque registrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name", output.ProductId);
            return View(output);
        }

        // GET: Stock/Adjust - Ajuste de estoque
        public async Task<IActionResult> Adjust()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "Name");
            return View();
        }

        // POST: Stock/Adjust
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(int productId, int newQuantity, string reason)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Produto não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var oldQuantity = product.Stock;
            var difference = newQuantity - oldQuantity;
            
            product.Stock = newQuantity;
            _context.Update(product);

            // Criar movimentação
            var movement = new StockMovement
            {
                ProductId = productId,
                MovementDate = DateTime.Now,
                MovementType = "adjustment",
                Quantity = Math.Abs(difference),
                Reason = reason,
                UserId = User.Identity.Name
            };

            _context.Add(movement);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Ajuste de estoque realizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Stock/LowStock - Produtos com estoque baixo
        public async Task<IActionResult> LowStock()
        {
            var products = await _context.Products
                .Include(p => p.ProductCategory)
                .Where(p => p.Stock <= p.MinimumStockLevel)
                .OrderBy(p => p.Stock)
                .ToListAsync();
            
            return View(products);
        }
    }
}

