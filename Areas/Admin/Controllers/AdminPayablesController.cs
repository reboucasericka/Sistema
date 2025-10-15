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
    public class AdminPayablesController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;

        public AdminPayablesController(SistemaDbContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: Admin/Payables
        public async Task<IActionResult> Index(string? status, string? type, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Payables
                .Include(p => p.Professional)
                .Include(p => p.Supplier)
                .Include(p => p.PaymentMethod)
                .Include(p => p.User)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (startDate.HasValue)
            {
                query = query.Where(p => p.DueDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.DueDate <= endDate.Value);
            }

            var payables = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            ViewBag.Status = status;
            ViewBag.Type = type;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

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
                .Include(p => p.Professional)
                .Include(p => p.Supplier)
                .Include(p => p.PaymentMethod)
                .Include(p => p.User)
                .Include(p => p.Sale)
                .FirstOrDefaultAsync(m => m.PayableId == id);

            if (payable == null)
            {
                return NotFound();
            }

            return View(payable);
        }

        // GET: Admin/Payables/Create
        public async Task<IActionResult> Create()
        {
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name");
            ViewData["Type"] = new SelectList(new[] { "Expense", "Commission", "Supplier", "Other" });
            return View();
        }

        // POST: Admin/Payables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Amount,DueDate,Type,ProfessionalId,SupplierId,PaymentMethodId,Notes")] Payable payable)
        {
            if (ModelState.IsValid)
            {
                payable.UserId = _userHelper.GetUserId(User);
                payable.CreatedAt = DateTime.Now;
                payable.Status = "Pending";
                payable.IsPaid = false;

                _context.Add(payable);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Pagamento criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", payable.ProfessionalId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", payable.SupplierId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name", payable.PaymentMethodId);
            ViewData["Type"] = new SelectList(new[] { "Expense", "Commission", "Supplier", "Other" }, payable.Type);
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

            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", payable.ProfessionalId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", payable.SupplierId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name", payable.PaymentMethodId);
            ViewData["Type"] = new SelectList(new[] { "Expense", "Commission", "Supplier", "Other" }, payable.Type);
            return View(payable);
        }

        // POST: Admin/Payables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PayableId,Description,Amount,DueDate,Type,ProfessionalId,SupplierId,PaymentMethodId,Status,IsPaid,PaymentDate")] Payable payable)
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
                    TempData["SuccessMessage"] = "Pagamento atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PayableExists(payable.PayableId))
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

            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", payable.ProfessionalId);
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "Name", payable.SupplierId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name", payable.PaymentMethodId);
            ViewData["Type"] = new SelectList(new[] { "Expense", "Commission", "Supplier", "Other" }, payable.Type);
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
                .Include(p => p.Professional)
                .Include(p => p.Supplier)
                .Include(p => p.PaymentMethod)
                .Include(p => p.User)
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
                TempData["SuccessMessage"] = "Pagamento excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Payables/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var payable = await _context.Payables.FindAsync(id);
            if (payable == null)
            {
                return NotFound();
            }

            payable.Status = "Paid";
            payable.IsPaid = true;
            payable.PaymentDate = DateTime.Now;
            payable.ClearUserId = _userHelper.GetUserId(User);

            _context.Update(payable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Pagamento marcado como pago!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Payables/Summary
        public async Task<IActionResult> Summary()
        {
            var summary = new
            {
                TotalPending = await _context.Payables.Where(p => p.Status == "Pending").SumAsync(p => p.Amount),
                TotalPaid = await _context.Payables.Where(p => p.Status == "Paid").SumAsync(p => p.Amount),
                TotalExpenses = await _context.Payables.Where(p => p.Type == "Expense").SumAsync(p => p.Amount),
                TotalCommissions = await _context.Payables.Where(p => p.Type == "Commission").SumAsync(p => p.Amount),
                OverdueCount = await _context.Payables.Where(p => p.Status == "Pending" && p.DueDate < DateTime.Now).CountAsync(),
                OverdueAmount = await _context.Payables.Where(p => p.Status == "Pending" && p.DueDate < DateTime.Now).SumAsync(p => p.Amount)
            };

            return Json(summary);
        }

        private bool PayableExists(int id)
        {
            return _context.Payables.Any(e => e.PayableId == id);
        }
    }
}