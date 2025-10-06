using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using Sistema.Data.Entities;
using Sistema.Data;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CashRegisterController : Controller
    {
        private readonly SistemaDbContext _context;

        public CashRegisterController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Admin/CashRegister
        public async Task<IActionResult> Index()
        {
            var cashRegisters = await _context.CashRegisters
                .Include(c => c.OpeningUser)
                .Include(c => c.ClosingUser)
                .OrderByDescending(c => c.Date)
                .ToListAsync();
            
            return View(cashRegisters);
        }

        // GET: Admin/CashRegister/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashRegister = await _context.CashRegisters
                .Include(c => c.OpeningUser)
                .Include(c => c.ClosingUser)
                .FirstOrDefaultAsync(m => m.CashRegisterId == id);
            
            if (cashRegister == null)
            {
                return NotFound();
            }

            return View(cashRegister);
        }

        // GET: Admin/CashRegister/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/CashRegister/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CashRegisterId,Date,InitialValue,FinalValue,OpeningUserId,Status,Notes")] CashRegister cashRegister)
        {
            if (ModelState.IsValid)
            {
                cashRegister.Date = DateTime.Now;
                cashRegister.Status = "aberto";
                cashRegister.OpeningUserId = 1; // You might need to adjust this based on your user system
                
                _context.Add(cashRegister);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cash register opened successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(cashRegister);
        }

        // GET: Admin/CashRegister/Close/5
        public async Task<IActionResult> Close(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashRegister = await _context.CashRegisters.FindAsync(id);
            if (cashRegister == null)
            {
                return NotFound();
            }

            return View(cashRegister);
        }

        // POST: Admin/CashRegister/Close/5
        [HttpPost, ActionName("Close")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseConfirmed(int id, decimal valorFinal)
        {
            var cashRegister = await _context.CashRegisters.FindAsync(id);
            if (cashRegister != null)
            {
                cashRegister.FinalValue = valorFinal;
                cashRegister.Status = "fechado";
                cashRegister.ClosingUserId = 1; // You might need to adjust this based on your user system
                
                _context.Update(cashRegister);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Cash register closed successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/CashRegister/DailyReport/5
        public async Task<IActionResult> DailyReport(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashRegister = await _context.CashRegisters
                .Include(c => c.OpeningUser)
                .Include(c => c.ClosingUser)
                .FirstOrDefaultAsync(m => m.CashRegisterId == id);

            if (cashRegister == null)
            {
                return NotFound();
            }

            // Get all movements for this cash register
            var movements = await _context.CashMovements
                .Where(m => m.CashRegisterId == id)
                .OrderBy(m => m.Date)
                .ToListAsync();

            // Calculate totals
            var totalEntries = movements.Where(m => m.Type == "entry").Sum(m => m.Amount);
            var totalExits = movements.Where(m => m.Type == "exit").Sum(m => m.Amount);
            var expectedFinal = cashRegister.InitialValue + totalEntries - totalExits;
            var difference = cashRegister.FinalValue - expectedFinal;

            ViewBag.Movements = movements;
            ViewBag.TotalEntries = totalEntries;
            ViewBag.TotalExits = totalExits;
            ViewBag.ExpectedFinal = expectedFinal;
            ViewBag.Difference = difference;

            return View(cashRegister);
        }

        private bool CashRegisterExists(int id)
        {
            return _context.CashRegisters.Any(e => e.CashRegisterId == id);
        }
    }
}