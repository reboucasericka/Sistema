using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PayablesController : Controller
    {
        private readonly SistemaDbContext _context;

        public PayablesController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Payables
        public async Task<IActionResult> Index()
        {
            var payables = await _context.Payables
                .Include(p => p.LaunchUser)
                .Include(p => p.ClearUser)
                .OrderByDescending(p => p.DueDate)
                .ToListAsync();
            
            return View(payables);
        }

        // GET: Admin/Payables/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payable = await _context.Payables
                .Include(p => p.LaunchUser)
                .Include(p => p.ClearUser)
                .FirstOrDefaultAsync(m => m.PayableId == id);
            
            if (payable == null)
            {
                return NotFound();
            }

            return View(payable);
        }

        // GET: Admin/Payables/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Payables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PayableId,Description,Type,Value,LaunchDate,DueDate,LaunchUser,IsPaid")] Payable payable)
        {
            if (ModelState.IsValid)
            {
                payable.LaunchDate = DateTime.Now;
                payable.IsPaid = false;
                payable.UserId = 1; // You might need to adjust this based on your user system
                
                _context.Add(payable);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Payableable created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(payable);
        }

        // GET: Admin/Payables/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payable = await _context.Payables.FindAsync(id);
            if (payable == null)
            {
                return NotFound();
            }
            return View(payable);
        }

        // POST: Admin/Payables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PayableId,Description,Type,Value,LaunchDate,DueDate,LaunchUser,IsPaid,PaymentDate")] Payable payable)
        {
            if (id != payable.PayableId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payable);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Payableable updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PayableableExists(payable.PayableId))
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
            return View(payable);
        }

        // GET: Admin/Payables/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payable = await _context.Payables
                .Include(p => p.LaunchUser)
                .FirstOrDefaultAsync(m => m.PayableId == id);
            if (payable == null)
            {
                return NotFound();
            }

            return View(payable);
        }

        // POST: Admin/Payables/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payable = await _context.Payables.FindAsync(id);
            if (payable != null)
            {
                _context.Payables.Remove(payable);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Payableable deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Payables/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var payable = await _context.Payables.FindAsync(id);
            if (payable != null)
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

                // Mark payable as paid
                payable.IsPaid = true;
                payable.PaymentDate = DateTime.Now;
                payable.ClearUserId = 1; // You might need to adjust this based on your user system
                
                _context.Update(payable);

                // Create cash movement for the payment (exit from cash)
                var cashMovement = new CashMovement
                {
                    CashRegisterId = openCashRegister.CashRegisterId,
                    Type = "exit",
                    Amount = payable.Value,
                    Description = $"Payablement made - {payable.Description}",
                    Date = DateTime.Now,
                    Reference = $"Payableable-{payable.PayableId}",
                    RelatedEntityId = payable.PayableId,
                    RelatedEntityType = "Payableable"
                };

                _context.CashMovements.Add(cashMovement);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = $"Payableable marked as paid and €{payable.Value:N2} deducted from cash register!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PayableableExists(int id)
        {
            return _context.Payables.Any(e => e.PayableId == id);
        }
    }
}
