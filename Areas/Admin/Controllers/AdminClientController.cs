using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Helpers;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminClientController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;

        public AdminClientController(SistemaDbContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        // GET: Perfil do Cliente
        public async Task<IActionResult> Profile()
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name) 
                      ?? await _userHelper.GetUserByUsernameAsync(User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Buscar agendamentos do cliente
            var appointments = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .Where(a => a.Client.Email == user.Email)
                .OrderByDescending(a => a.StartTime.Date)
                .ThenByDescending(a => a.StartTime.TimeOfDay)
                .ToListAsync();

            var model = new AdminClientProfileViewModel
            {
                User = user,
                Appointments = appointments,
                PendingAppointments = appointments.Where(a => a.StartTime.Date >= DateTime.Today && a.Status == "Pendente").ToList(),
                ConfirmedAppointments = appointments.Where(a => a.StartTime.Date >= DateTime.Today && a.Status == "Confirmado").ToList(),
                PastAppointments = appointments.Where(a => a.StartTime.Date < DateTime.Today).ToList()
            };

            return View(model);
        }

        // POST: Confirmar Agendamento
        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Agendamento não encontrado." });
            }

            // Verificar se o agendamento pertence ao usuário logado
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (appointment.Client.Email != user.Email)
            {
                return Json(new { success = false, message = "Você não tem permissão para confirmar este agendamento." });
            }

            appointment.Status = "Confirmado";
            appointment.ReminderSent = true;
            
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Agendamento confirmado com sucesso!" });
        }

        // POST: Cancelar Agendamento
        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Agendamento não encontrado." });
            }

            // Verificar se o agendamento pertence ao usuário logado
            var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
            if (appointment.Client.Email != user.Email)
            {
                return Json(new { success = false, message = "Você não tem permissão para cancelar este agendamento." });
            }

            appointment.Status = "Cancelado";
            
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Agendamento cancelado com sucesso!" });
        }

        // GET: Novo Agendamento
        public async Task<IActionResult> NewAppointment()
        {
            var services = await _context.Service
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .ToListAsync();

            var professionals = await _context.Professionals
                .Where(p => p.IsActive)
                .ToListAsync();

            ViewBag.Services = services;
            ViewBag.Professionals = professionals;

            return View();
        }

        // POST: Criar Novo Agendamento
        [HttpPost]
        public async Task<IActionResult> NewAppointment(AdminCreateAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Buscar ou criar cliente
                var client = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == user.Email);

                if (client == null)
                {
                    client = new Customer
                    {
                        Name = $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Phone = user.PhoneNumber,
                        RegistrationDate = DateTime.Now,
                        IsActive = true
                    };
                    _context.Customers.Add(client);
                    await _context.SaveChangesAsync();
                }

                var appointment = new Appointment
                {
                    CustomerId = client.CustomerId,
                    ServiceId = model.ServiceId,
                    ProfessionalId = model.ProfessionalId,
                    StartTime = model.Date.Add(model.Time),
                    Status = "Pendente",
                    Notes = model.Notes,
                    ReminderSent = false,
                    ExportedToExcel = false,
                    ExportedToPdf = false
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Agendamento criado com sucesso! Aguarde a confirmação.";
                return RedirectToAction("Profile");
            }

            // Recarregar dados para a view
            var services = await _context.Service
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .ToListAsync();

            var professionals = await _context.Professionals
                .Where(p => p.IsActive)
                .ToListAsync();

            ViewBag.Services = services;
            ViewBag.Professionals = professionals;

            return View(model);
        }
    }
}


