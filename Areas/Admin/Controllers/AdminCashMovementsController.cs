using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminCashMovementsController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;

        public AdminCashMovementsController(SistemaDbContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: Admin/CashMovements
        public async Task<IActionResult> Index(string? type, DateTime? startDate, DateTime? endDate, int? cashRegisterId)
        {
            var query = _context.CashMovements
                .Include(cm => cm.CashRegister)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(cm => cm.Type == type);
            }

            if (startDate.HasValue)
            {
                query = query.Where(cm => cm.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(cm => cm.Date <= endDate.Value);
            }

            if (cashRegisterId.HasValue)
            {
                query = query.Where(cm => cm.CashRegisterId == cashRegisterId.Value);
            }

            var cashMovements = await query.OrderByDescending(cm => cm.Date).ToListAsync();

            // Calcular totais
            var totalEntradas = await query.Where(cm => cm.Type == "Entrada").SumAsync(cm => cm.Amount);
            var totalSaidas = await query.Where(cm => cm.Type == "Saída").SumAsync(cm => cm.Amount);
            var saldoLiquido = totalEntradas - totalSaidas;

            ViewBag.Type = type;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.CashRegisterId = cashRegisterId;
            ViewBag.TotalEntradas = totalEntradas;
            ViewBag.TotalSaidas = totalSaidas;
            ViewBag.SaldoLiquido = saldoLiquido;
            ViewBag.CashRegisters = new SelectList(_context.CashRegisters, "CashRegisterId", "Date");

            return View(cashMovements);
        }

        // GET: Admin/CashMovements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashMovement = await _context.CashMovements
                .Include(cm => cm.CashRegister)
                .FirstOrDefaultAsync(m => m.CashMovementId == id);

            if (cashMovement == null)
            {
                return NotFound();
            }

            return View(cashMovement);
        }

        // GET: Admin/CashMovements/Create
        public async Task<IActionResult> Create()
        {
            // Buscar caixa aberto
            var openCashRegister = await _context.CashRegisters
                .Where(cr => !cr.IsClosed)
                .OrderByDescending(cr => cr.Date)
                .FirstOrDefaultAsync();

            if (openCashRegister == null)
            {
                TempData["ErrorMessage"] = "Não há caixa aberto. Abra um caixa antes de registrar movimentações.";
                return RedirectToAction("Index", "AdminCashRegister");
            }

            ViewData["CashRegisterId"] = openCashRegister.CashRegisterId;
            ViewData["Type"] = new SelectList(new[] { "Entrada", "Saída" });
            return View();
        }

        // POST: Admin/CashMovements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CashRegisterId,Type,Amount,Description,ReferenceId,ReferenceType")] CashMovement cashMovement)
        {
            if (ModelState.IsValid)
            {
                cashMovement.Date = DateTime.Now;
                _context.Add(cashMovement);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Movimentação registrada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CashRegisterId"] = cashMovement.CashRegisterId;
            ViewData["Type"] = new SelectList(new[] { "Entrada", "Saída" }, cashMovement.Type);
            return View(cashMovement);
        }

        // GET: Admin/CashMovements/Edit/5
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

            ViewData["CashRegisterId"] = new SelectList(_context.CashRegisters, "CashRegisterId", "Date", cashMovement.CashRegisterId);
            ViewData["Type"] = new SelectList(new[] { "Entrada", "Saída" }, cashMovement.Type);
            return View(cashMovement);
        }

        // POST: Admin/CashMovements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CashMovementId,CashRegisterId,Type,Amount,Description,Date,ReferenceId,ReferenceType")] CashMovement cashMovement)
        {
            if (id != cashMovement.CashMovementId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cashMovement);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Movimentação atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CashMovementExists(cashMovement.CashMovementId))
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

            ViewData["CashRegisterId"] = new SelectList(_context.CashRegisters, "CashRegisterId", "Date", cashMovement.CashRegisterId);
            ViewData["Type"] = new SelectList(new[] { "Entrada", "Saída" }, cashMovement.Type);
            return View(cashMovement);
        }

        // GET: Admin/CashMovements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cashMovement = await _context.CashMovements
                .Include(cm => cm.CashRegister)
                .FirstOrDefaultAsync(m => m.CashMovementId == id);

            if (cashMovement == null)
            {
                return NotFound();
            }

            return View(cashMovement);
        }

        // POST: Admin/CashMovements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cashMovement = await _context.CashMovements.FindAsync(id);
            if (cashMovement != null)
            {
                _context.CashMovements.Remove(cashMovement);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Movimentação excluída com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/CashMovements/Summary
        public async Task<IActionResult> Summary(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.CashMovements.AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(cm => cm.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(cm => cm.Date <= endDate.Value);
            }

            var summary = new
            {
                TotalEntradas = await query.Where(cm => cm.Type == "Entrada").SumAsync(cm => cm.Amount),
                TotalSaidas = await query.Where(cm => cm.Type == "Saída").SumAsync(cm => cm.Amount),
                SaldoLiquido = await query.Where(cm => cm.Type == "Entrada").SumAsync(cm => cm.Amount) - 
                              await query.Where(cm => cm.Type == "Saída").SumAsync(cm => cm.Amount),
                TotalMovements = await query.CountAsync(),
                EntradasCount = await query.Where(cm => cm.Type == "Entrada").CountAsync(),
                SaidasCount = await query.Where(cm => cm.Type == "Saída").CountAsync()
            };

            return Json(summary);
        }

        private bool CashMovementExists(int id)
        {
            return _context.CashMovements.Any(e => e.CashMovementId == id);
        }
    }
}