using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using Sistema.Data.Entities;
using Sistema.Data;
using Microsoft.AspNetCore.Authorization;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminCashMovementsController : Controller
    {
        private readonly SistemaDbContext _context;

        public AdminCashMovementsController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: CashMovements
        public async Task<IActionResult> Index(int? cashRegisterId)
        {
            var query = _context.CashMovements
                .Include(cm => cm.CashRegister)
                .AsQueryable();

            if (cashRegisterId.HasValue)
            {
                query = query.Where(cm => cm.CashRegisterId == cashRegisterId.Value);
                ViewBag.CashRegisterId = cashRegisterId.Value;
            }

            var movements = await query
                .OrderByDescending(cm => cm.Date)
                .ToListAsync();

            return View(movements);
        }

        // GET: CashMovements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashMovement = await _context.CashMovements
                .Include(cm => cm.CashRegister)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashMovement == null)
            {
                return NotFound();
            }

            return View(cashMovement);
        }

        // GET: CashMovements/Create
        public async Task<IActionResult> Create(int? cashRegisterId)
        {
            // Get open cash registers
            var openCashRegisters = await _context.CashRegisters
                .Where(cr => cr.Status == "aberto")
                .OrderByDescending(cr => cr.Date)
                .ToListAsync();

            ViewData["CashRegisterId"] = new SelectList(openCashRegisters, "CashRegisterId", "Date", cashRegisterId);
            
            return View();
        }

        // POST: CashMovements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CashRegisterId,Type,Amount,Description,Date,Reference,RelatedEntityId,RelatedEntityType")] CashMovement cashMovement)
        {
            if (ModelState.IsValid)
            {
                cashMovement.Date = DateTime.Now;
                _context.Add(cashMovement);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cash movement created successfully!";
                return RedirectToAction(nameof(Index), new { cashRegisterId = cashMovement.CashRegisterId });
            }

            var openCashRegisters = await _context.CashRegisters
                .Where(cr => cr.Status == "aberto")
                .OrderByDescending(cr => cr.Date)
                .ToListAsync();

            ViewData["CashRegisterId"] = new SelectList(openCashRegisters, "CashRegisterId", "Date", cashMovement.CashRegisterId);
            return View(cashMovement);
        }

        // GET: CashMovements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashMovement = await _context.CashMovements.FindAsync(id);
            if (cashMovement == null)
            {
                return NotFound();
            }

            var openCashRegisters = await _context.CashRegisters
                .Where(cr => cr.Status == "aberto")
                .OrderByDescending(cr => cr.Date)
                .ToListAsync();

            ViewData["CashRegisterId"] = new SelectList(openCashRegisters, "CashRegisterId", "Date", cashMovement.CashRegisterId);
            return View(cashMovement);
        }

        // POST: CashMovements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CashRegisterId,Type,Amount,Description,Date,Reference,RelatedEntityId,RelatedEntityType")] CashMovement cashMovement)
        {
            if (id != cashMovement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cashMovement);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Cash movement updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CashMovementExists(cashMovement.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { cashRegisterId = cashMovement.CashRegisterId });
            }

            var openCashRegisters = await _context.CashRegisters
                .Where(cr => cr.Status == "aberto")
                .OrderByDescending(cr => cr.Date)
                .ToListAsync();

            ViewData["CashRegisterId"] = new SelectList(openCashRegisters, "CashRegisterId", "Date", cashMovement.CashRegisterId);
            return View(cashMovement);
        }

        // GET: CashMovements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashMovement = await _context.CashMovements
                .Include(cm => cm.CashRegister)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (cashMovement == null)
            {
                return NotFound();
            }

            return View(cashMovement);
        }

        // POST: CashMovements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cashMovement = await _context.CashMovements.FindAsync(id);
            if (cashMovement != null)
            {
                _context.CashMovements.Remove(cashMovement);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cash movement deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CashMovementExists(int id)
        {
            return _context.CashMovements.Any(e => e.Id == id);
        }
    }
}
