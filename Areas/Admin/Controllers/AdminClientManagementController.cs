using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminClientManagementController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;

        public AdminClientManagementController(SistemaDbContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: Lista de Customeres
        public async Task<IActionResult> Index(string? search, string? sortBy, string? sortOrder)
        {
            ViewData["Title"] = "Gestão de Customeres";
            
            var query = _context.Customers.AsQueryable();

            // Filtro de busca
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => 
                    c.Name.Contains(search) || 
                    c.Email.Contains(search) || 
                    c.Phone.Contains(search));
            }

            // Ordenação
            query = sortBy switch
            {
                "name" => sortOrder == "desc" ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
                "email" => sortOrder == "desc" ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
                "registration" => sortOrder == "desc" ? query.OrderByDescending(c => c.RegistrationDate) : query.OrderBy(c => c.RegistrationDate),
                "status" => sortOrder == "desc" ? query.OrderByDescending(c => c.IsActive) : query.OrderBy(c => c.IsActive),
                _ => query.OrderByDescending(c => c.RegistrationDate)
            };

            var clients = await query.ToListAsync();

            // Estatísticas
            ViewBag.TotalCustomers = clients.Count;
            ViewBag.ActiveCustomers = clients.Count(c => c.IsActive);
            ViewBag.InactiveCustomers = clients.Count(c => !c.IsActive);
            ViewBag.NewThisMonth = clients.Count(c => c.RegistrationDate >= DateTime.Today.AddDays(-30));

            return View(clients);
        }

        // GET: Detalhes do Customere
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (client == null)
            {
                return NotFound();
            }

            // Buscar agendamentos do cliente
            var appointments = await _context.Appointments
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .Where(a => a.CustomerId == id)
                .OrderByDescending(a => a.StartTime.Date)
                .ToListAsync();

            // Buscar histórico de compras/faturação
            var billingHistory = await _context.Billings
                .Include(b => b.User)
                .Where(b => b.User.Email == client.Email)
                .OrderByDescending(b => b.Date)
                .ToListAsync();

            // Buscar recebíveis do cliente
            var receivables = await _context.Receivables
                .Include(r => r.PaymentMethod)
                .Where(r => r.PersonId == id)
                .OrderByDescending(r => r.LaunchDate)
                .ToListAsync();

            ViewBag.Appointments = appointments;
            ViewBag.BillingHistory = billingHistory;
            ViewBag.Receivables = receivables;

            return View(client);
        }

        // GET: Criar Customere
        public IActionResult Create()
        {
            return View();
        }

        // POST: Criar Customere
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer client)
        {
            if (ModelState.IsValid)
            {
                // Verificar se email já existe
                if (!string.IsNullOrEmpty(client.Email))
                {
                    var existingCustomer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.Email == client.Email);
                    
                    if (existingCustomer != null)
                    {
                        ModelState.AddModelError("Email", "Já existe um cliente com este email.");
                        return View(client);
                    }
                }

                client.RegistrationDate = DateTime.Now;
                client.IsActive = true;

                _context.Add(client);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Customere criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(client);
        }

        // GET: Editar Customere
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Customers.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Editar Customere
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer client)
        {
            if (id != client.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Customere atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(client.CustomerId))
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

            return View(client);
        }

        // GET: Deletar Customere
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Customers.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            // Verificar se tem agendamentos ativos
            var hasActiveAppointments = await _context.Appointments
                .AnyAsync(a => a.CustomerId == id && a.StartTime.Date >= DateTime.Today);

            ViewBag.HasActiveAppointments = hasActiveAppointments;

            return View(client);
        }

        // POST: Deletar Customere
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Customers.FindAsync(id);
            if (client != null)
            {
                // Verificar se tem agendamentos ativos
                var hasActiveAppointments = await _context.Appointments
                    .AnyAsync(a => a.CustomerId == id && a.StartTime.Date >= DateTime.Today);

                if (hasActiveAppointments)
                {
                    // Desativar em vez de deletar
                    client.IsActive = false;
                    _context.Update(client);
                    TempData["WarningMessage"] = "Customere desativado (possui agendamentos ativos).";
                }
                else
                {
                    _context.Customers.Remove(client);
                    TempData["SuccessMessage"] = "Customere removido com sucesso!";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Ativar/Desativar Customere
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var client = await _context.Customers.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            client.IsActive = !client.IsActive;
            _context.Update(client);
            await _context.SaveChangesAsync();

            var status = client.IsActive ? "ativado" : "desativado";
            TempData["SuccessMessage"] = $"Customere {status} com sucesso!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Relatório de Customeres
        public async Task<IActionResult> Report()
        {
            var clients = await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Estatísticas para o relatório
            ViewBag.TotalCustomers = clients.Count;
            ViewBag.ActiveCustomers = clients.Count(c => c.IsActive);
            ViewBag.NewThisMonth = clients.Count(c => c.RegistrationDate >= DateTime.Today.AddDays(-30));
            ViewBag.NewThisWeek = clients.Count(c => c.RegistrationDate >= DateTime.Today.AddDays(-7));

            return View(clients);
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
