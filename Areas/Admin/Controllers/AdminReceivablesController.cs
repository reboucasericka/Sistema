using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminReceivablesController : Controller
    {
        private readonly SistemaDbContext _context;

        public AdminReceivablesController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Receivables
        public async Task<IActionResult> Index()
        {
            var receivables = await _context.Receivables
                .Include(r => r.LaunchUser)
                .Include(r => r.ClearUser)
                .OrderByDescending(r => r.DueDate)
                .ToListAsync();
            
            return View(receivables);
        }

        // GET: Admin/Receivables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receivable = await _context.Receivables
                .Include(r => r.LaunchUser)
                .Include(r => r.ClearUser)
                .FirstOrDefaultAsync(m => m.ReceivableId == id);
            
            if (receivable == null)
            {
                return NotFound();
            }

            return View(receivable);
        }

        // GET: Admin/Receivables/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Receivables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReceivableId,Description,Type,Value,LaunchDate,DueDate,LaunchUser,IsPaid")] Receivable receivable)
        {
            if (ModelState.IsValid)
            {
                receivable.LaunchDate = DateTime.Now;
                receivable.IsPaid = false;
                receivable.UserId = 1; // You might need to adjust this based on your user system
                
                _context.Add(receivable);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Receivable created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(receivable);
        }

        // GET: Admin/Receivables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receivable = await _context.Receivables.FindAsync(id);
            if (receivable == null)
            {
                return NotFound();
            }
            return View(receivable);
        }

        // POST: Admin/Receivables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReceivableId,Description,Type,Value,LaunchDate,DueDate,LaunchUser,IsPaid,PaymentDate")] Receivable receivable)
        {
            if (id != receivable.ReceivableId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(receivable);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Receivable updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReceivableExists(receivable.ReceivableId))
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
            return View(receivable);
        }

        // GET: Admin/Receivables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var receivable = await _context.Receivables
                .Include(r => r.LaunchUser)
                .FirstOrDefaultAsync(m => m.ReceivableId == id);
            if (receivable == null)
            {
                return NotFound();
            }

            return View(receivable);
        }

        // POST: Admin/Receivables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var receivable = await _context.Receivables.FindAsync(id);
            if (receivable != null)
            {
                _context.Receivables.Remove(receivable);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Receivable deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Receivables/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var receivable = await _context.Receivables.FindAsync(id);
            if (receivable != null)
            {
                // Check if there's an open cash register
                var openCashRegister = await _context.CashRegisters
                    .Where(c => c.Status == "aberto")
                    .OrderByDescending(c => c.Date)
                    .FirstOrDefaultAsync();

                if (openCashRegister == null)
                {
                    TempData["ErrorMessage"] = "No open cash register found. Please open a cash register first.";
                    return RedirectToAction(nameof(Index));
                }

                // Mark receivable as paid
                receivable.IsPaid = true;
                receivable.PaymentDate = DateTime.Now;
                receivable.ClearUserId = 1; // You might need to adjust this based on your user system
                
                _context.Update(receivable);

                // Create cash movement for the payment
                var cashMovement = new CashMovement
                {
                    CashRegisterId = openCashRegister.CashRegisterId,
                    Type = "entry",
                    Amount = receivable.Value,
                    Description = $"Payment received - {receivable.Description}",
                    Date = DateTime.Now,
                    Reference = $"Receivable-{receivable.ReceivableId}",
                    RelatedEntityId = receivable.ReceivableId,
                    RelatedEntityType = "Receivable"
                };

                _context.CashMovements.Add(cashMovement);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Receivable marked as paid and €{receivable.Value:N2} added to cash register!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReceivableExists(int id)
        {
            return _context.Receivables.Any(e => e.ReceivableId == id);
        }
    }
}
