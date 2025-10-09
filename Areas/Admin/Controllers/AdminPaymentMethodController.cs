using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminPaymentMethodController : Controller
    {
        private readonly SistemaDbContext _context;

        public AdminPaymentMethodController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Lista de Métodos de Pagamento
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Métodos de Pagamento";

            var paymentMethods = await _context.PaymentMethods
                .OrderBy(p => p.Description)
                .ToListAsync();

            return View(paymentMethods);
        }

        // GET: Detalhes do Método de Pagamento
        public async Task<IActionResult> Details(int id)
        {
            var paymentMethod = await _context.PaymentMethods
                .Include(p => p.Payables)
                .Include(p => p.Receivables)
                .FirstOrDefaultAsync(p => p.PaymentMethodId == id);

            if (paymentMethod == null)
            {
                return NotFound();
            }

            // Estatísticas de uso
            ViewBag.TotalPayments = paymentMethod.Payables.Count;
            ViewBag.TotalReceivables = paymentMethod.Receivables.Count;
            ViewBag.TotalValuePaid = paymentMethod.Payables.Sum(p => p.Value);
            ViewBag.TotalValueReceived = paymentMethod.Receivables.Sum(r => r.Value);

            return View(paymentMethod);
        }

        // GET: Criar Método de Pagamento
        public IActionResult Create()
        {
            return View();
        }

        // POST: Criar Método de Pagamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentMethod paymentMethod)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paymentMethod);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Método de pagamento criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(paymentMethod);
        }

        // GET: Editar Método de Pagamento
        public async Task<IActionResult> Edit(int id)
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            return View(paymentMethod);
        }

        // POST: Editar Método de Pagamento
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentMethod paymentMethod)
        {
            if (id != paymentMethod.PaymentMethodId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paymentMethod);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Método de pagamento atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentMethodExists(paymentMethod.PaymentMethodId))
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

            return View(paymentMethod);
        }

        // GET: Deletar Método de Pagamento
        public async Task<IActionResult> Delete(int id)
        {
            var paymentMethod = await _context.PaymentMethods
                .Include(p => p.Payables)
                .Include(p => p.Receivables)
                .FirstOrDefaultAsync(p => p.PaymentMethodId == id);

            if (paymentMethod == null)
            {
                return NotFound();
            }

            // Verificar se tem transações associadas
            var hasTransactions = paymentMethod.Payables.Any() || paymentMethod.Receivables.Any();
            ViewBag.HasTransactions = hasTransactions;

            return View(paymentMethod);
        }

        // POST: Deletar Método de Pagamento
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod != null)
            {
                // Verificar se tem transações associadas
                var hasTransactions = await _context.Payables.AnyAsync(p => p.PaymentMethodId == id) ||
                                    await _context.Receivables.AnyAsync(r => r.PaymentMethodId == id);

                if (hasTransactions)
                {
                    // Desativar em vez de deletar
                    paymentMethod.IsActive = false;
                    _context.Update(paymentMethod);
                    TempData["WarningMessage"] = "Método de pagamento desativado (possui transações associadas).";
                }
                else
                {
                    _context.PaymentMethods.Remove(paymentMethod);
                    TempData["SuccessMessage"] = "Método de pagamento removido com sucesso!";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Ativar/Desativar Método de Pagamento
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod == null)
            {
                return NotFound();
            }

            paymentMethod.IsActive = !paymentMethod.IsActive;
            _context.Update(paymentMethod);
            await _context.SaveChangesAsync();

            var status = paymentMethod.IsActive ? "ativado" : "desativado";
            TempData["SuccessMessage"] = $"Método de pagamento {status} com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        private bool PaymentMethodExists(int id)
        {
            return _context.PaymentMethods.Any(e => e.PaymentMethodId == id);
        }
    }
}
