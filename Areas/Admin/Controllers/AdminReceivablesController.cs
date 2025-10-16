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

        /// <summary>
        /// Exibe a lista de recebimentos com filtros opcionais
        /// </summary>
        /// <param name="status">Status do recebimento (Pending, Paid, Todos)</param>
        /// <param name="customerId">ID do cliente para filtrar</param>
        /// <param name="professionalId">ID do profissional para filtrar</param>
        /// <param name="startDate">Data de início para filtrar</param>
        /// <param name="endDate">Data de fim para filtrar</param>
        /// <returns>View com lista de recebimentos</returns>
        public async Task<IActionResult> Index(string? status, int? customerId, int? professionalId, DateTime? startDate, DateTime? endDate)
        {
            // Inicializar ViewBags com valores padrão para evitar NullReferenceException
            ViewBag.Status = status ?? "Todos";
            ViewBag.CustomerId = customerId;
            ViewBag.ProfessionalId = professionalId;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;

            // Carregar listas para filtros com proteção contra nulls
            var customers = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User != null && !string.IsNullOrEmpty(c.User.FirstName))
                .OrderBy(c => c.User.FirstName)
                .ToListAsync();

            var professionals = await _context.Professionals
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.Customers = new SelectList(customers, "CustomerId", "User.FirstName");
            ViewBag.Professionals = new SelectList(professionals, "ProfessionalId", "Name");

            // Construir query base com todos os relacionamentos necessários
            var query = _context.Receivables
                .Include(r => r.Customer)
                    .ThenInclude(c => c.User)
                .Include(r => r.Professional)
                .Include(r => r.Service)
                .Include(r => r.Sale)
                .Include(r => r.PaymentMethod)
                .Include(r => r.User)
                .AsQueryable();

            // Aplicar filtros de forma segura, evitando nulls
            if (!string.IsNullOrEmpty(status) && status != "Todos")
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

            // Executar query e retornar resultados ordenados por data de criação
            var receivables = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
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

        /// <summary>
        /// Exibe formulário para criar novo recebimento
        /// </summary>
        /// <returns>View com formulário de criação</returns>
        public async Task<IActionResult> Create()
        {
            // Carregar dados para dropdowns com proteção contra nulls
            var customers = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User != null && !string.IsNullOrEmpty(c.User.FirstName))
                .OrderBy(c => c.User.FirstName)
                .ToListAsync();

            var professionals = await _context.Professionals
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .OrderBy(p => p.Name)
                .ToListAsync();

            var services = await _context.Services
                .Where(s => !string.IsNullOrEmpty(s.Name))
                .OrderBy(s => s.Name)
                .ToListAsync();

            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.Name)
                .ToListAsync();

            // Inicializar ViewData com proteção contra nulls
            ViewData["CustomerId"] = new SelectList(customers, "CustomerId", "User.FirstName");
            ViewData["ProfessionalId"] = new SelectList(professionals, "ProfessionalId", "Name");
            ViewData["ServiceId"] = new SelectList(services, "ServiceId", "Name");
            ViewData["PaymentMethodId"] = new SelectList(paymentMethods, "PaymentMethodId", "Name");
            
            return View();
        }

        /// <summary>
        /// Processa a criação de um novo recebimento
        /// </summary>
        /// <param name="receivable">Dados do recebimento</param>
        /// <returns>Redirect para Index ou View com erros</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Description,Amount,CustomerId,ProfessionalId,ServiceId,PaymentMethodId")] Receivable receivable)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obter ID do usuário com verificação de null
                    var userId = _userHelper.GetUserId(User);
                    if (userId == null)
                    {
                        ModelState.AddModelError("", "Usuário não encontrado. Faça login novamente.");
                        return await CreateWithViewData(receivable);
                    }

                    // Configurar dados do recebimento
                    receivable.UserId = userId;
                    receivable.CreatedAt = DateTime.Now;
                    receivable.LaunchDate = DateTime.Now;
                    receivable.Status = "Pending";
                    receivable.IsPaid = false;

                    _context.Add(receivable);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Recebimento criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Erro ao criar recebimento: {ex.Message}");
                    return await CreateWithViewData(receivable);
                }
            }

            return await CreateWithViewData(receivable);
        }

        /// <summary>
        /// Método auxiliar para recarregar ViewData em caso de erro
        /// </summary>
        private async Task<IActionResult> CreateWithViewData(Receivable receivable)
        {
            // Carregar dados para dropdowns com proteção contra nulls
            var customers = await _context.Customers
                .Include(c => c.User)
                .Where(c => c.User != null && !string.IsNullOrEmpty(c.User.FirstName))
                .OrderBy(c => c.User.FirstName)
                .ToListAsync();

            var professionals = await _context.Professionals
                .Where(p => !string.IsNullOrEmpty(p.Name))
                .OrderBy(p => p.Name)
                .ToListAsync();

            var services = await _context.Services
                .Where(s => !string.IsNullOrEmpty(s.Name))
                .OrderBy(s => s.Name)
                .ToListAsync();

            var paymentMethods = await _context.PaymentMethods
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.Name)
                .ToListAsync();

            // Inicializar ViewData com valores selecionados
            ViewData["CustomerId"] = new SelectList(customers, "CustomerId", "User.FirstName", receivable.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(professionals, "ProfessionalId", "Name", receivable.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(services, "ServiceId", "Name", receivable.ServiceId);
            ViewData["PaymentMethodId"] = new SelectList(paymentMethods, "PaymentMethodId", "Name", receivable.PaymentMethodId);
            
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
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", receivable.ServiceId);
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
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", receivable.ServiceId);
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

        /// <summary>
        /// Marca um recebimento como pago
        /// </summary>
        /// <param name="id">ID do recebimento</param>
        /// <returns>Redirect para Index</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            try
            {
                var receivable = await _context.Receivables.FindAsync(id);
                if (receivable == null)
                {
                    TempData["ErrorMessage"] = "Recebimento não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Obter ID do usuário com verificação de null
                var userId = _userHelper.GetUserId(User);
                if (userId == null)
                {
                    TempData["ErrorMessage"] = "Usuário não encontrado. Faça login novamente.";
                    return RedirectToAction(nameof(Index));
                }

                // Atualizar status do recebimento
                receivable.Status = "Paid";
                receivable.IsPaid = true;
                receivable.PaymentDate = DateTime.Now;
                receivable.ClearUserId = userId;

                _context.Update(receivable);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Recebimento marcado como pago!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao marcar recebimento como pago: {ex.Message}";
            }

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