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
    public class AdminReceivablesController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;

        public AdminReceivablesController(SistemaDbContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: Admin/Receivables
        public async Task<IActionResult> Index(string? status, int? customerId, int? professionalId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Receivables
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Include(r => r.Professional)
                .Include(r => r.Service)
                .Include(r => r.Sale)
                .Include(r => r.PaymentMethod)
                .Include(r => r.User)
                .AsQueryable();

            // Filtros
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (customerId.HasValue)
            {
                query = query.Where(r => r.CustomerId == customerId.Value);
            }

            if (professionalId.HasValue)
            {
                query = query.Where(r => r.ProfessionalId == professionalId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= endDate.Value);
            }

            var receivables = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            ViewBag.Status = status;
            ViewBag.CustomerId = customerId;
            ViewBag.ProfessionalId = professionalId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.Customers = new SelectList(_context.Customers.Include(c => c.User), "CustomerId", "User.FirstName");
            ViewBag.Professionals = new SelectList(_context.Professionals, "ProfessionalId", "Name");

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
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Include(r => r.Professional)
                .Include(r => r.Service)
                .Include(r => r.Sale)
                .Include(r => r.PaymentMethod)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.ReceivableId == id);

            if (receivable == null)
            {
                return NotFound();
            }

            return View(receivable);
        }

        // GET: Admin/Receivables/Create
        public async Task<IActionResult> Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers.Include(c => c.User), "CustomerId", "User.FirstName");
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name");
            ViewData["ServiceId"] = new SelectList(_context.Service, "ServiceId", "Name");
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name");
            return View();
        }

        // POST: Admin/Receivables/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Amount,CustomerId,ProfessionalId,ServiceId,PaymentMethodId")] Receivable receivable)
        {
            if (ModelState.IsValid)
            {
                receivable.UserId = _userHelper.GetUserId(User);
                receivable.CreatedAt = DateTime.Now;
                receivable.LaunchDate = DateTime.Now;
                receivable.Status = "Pending";
                receivable.IsPaid = false;

                _context.Add(receivable);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Recebimento criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers.Include(c => c.User), "CustomerId", "User.FirstName", receivable.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", receivable.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Service, "ServiceId", "Name", receivable.ServiceId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name", receivable.PaymentMethodId);
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

            ViewData["CustomerId"] = new SelectList(_context.Customers.Include(c => c.User), "CustomerId", "User.FirstName", receivable.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", receivable.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Service, "ServiceId", "Name", receivable.ServiceId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name", receivable.PaymentMethodId);
            return View(receivable);
        }

        // POST: Admin/Receivables/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReceivableId,Description,Amount,CustomerId,ProfessionalId,ServiceId,PaymentMethodId,Status,IsPaid,PaymentDate")] Receivable receivable)
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
                    TempData["SuccessMessage"] = "Recebimento atualizado com sucesso!";
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

            ViewData["CustomerId"] = new SelectList(_context.Customers.Include(c => c.User), "CustomerId", "User.FirstName", receivable.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", receivable.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Service, "ServiceId", "Name", receivable.ServiceId);
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods.Where(pm => pm.IsActive), "PaymentMethodId", "Name", receivable.PaymentMethodId);
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
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Include(r => r.Professional)
                .Include(r => r.Service)
                .Include(r => r.PaymentMethod)
                .Include(r => r.User)
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
                TempData["SuccessMessage"] = "Recebimento excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Receivables/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var receivable = await _context.Receivables.FindAsync(id);
            if (receivable == null)
            {
                return NotFound();
            }

            receivable.Status = "Paid";
            receivable.IsPaid = true;
            receivable.PaymentDate = DateTime.Now;
            receivable.ClearUserId = _userHelper.GetUserId(User);

            _context.Update(receivable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Recebimento marcado como pago!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Receivables/Summary
        public async Task<IActionResult> Summary()
        {
            var summary = new
            {
                TotalPending = await _context.Receivables.Where(r => r.Status == "Pending").SumAsync(r => r.Amount),
                TotalPaid = await _context.Receivables.Where(r => r.Status == "Paid").SumAsync(r => r.Amount),
                TotalServices = await _context.Receivables.Where(r => r.ServiceId != null).SumAsync(r => r.Amount),
                TotalSales = await _context.Receivables.Where(r => r.SaleId != null).SumAsync(r => r.Amount),
                OverdueCount = await _context.Receivables.Where(r => r.Status == "Pending" && r.CreatedAt < DateTime.Now.AddDays(-30)).CountAsync(),
                OverdueAmount = await _context.Receivables.Where(r => r.Status == "Pending" && r.CreatedAt < DateTime.Now.AddDays(-30)).SumAsync(r => r.Amount)
            };

            return Json(summary);
        }

        private bool ReceivableExists(int id)
        {
            return _context.Receivables.Any(e => e.ReceivableId == id);
        }
    }
}