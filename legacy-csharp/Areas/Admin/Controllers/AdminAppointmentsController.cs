using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using Sistema.Services;
using Sistema.Models;
using System.Linq;
using Microsoft.AspNetCore.Authorization;


namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAppointmentsController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IGoogleCalendarSyncService _calendarSyncService;
        private readonly ILogger<AdminAppointmentsController> _logger;

        public AdminAppointmentsController(
            SistemaDbContext context, 
            IAppointmentRepository appointmentRepository, 
            ICustomerRepository customerRepository,
            IGoogleCalendarSyncService calendarSyncService,
            ILogger<AdminAppointmentsController> logger)
        {
            _context = context;
            _appointmentRepository = appointmentRepository;
            _customerRepository = customerRepository;
            _calendarSyncService = calendarSyncService;
            _logger = logger;
        }


        // ✅ Página pública de agendamento online
        [AllowAnonymous] // Não pede login
        public async Task<IActionResult> Public()
        {
            ViewData["Title"] = "Agendamento Online";

            // Carregar os dados necessários
            ViewBag.Services = await _context.Services
                .Include(s => s.Category)
                .ToListAsync();

            ViewBag.Plans = await _context.Plans.ToListAsync();

            ViewBag.Professionals = await _context.Professionals
                .Include(p => p.User) // para exibir nome e foto do usuário vinculado
                .ToListAsync();

            ViewBag.Reviews = await _context.ServiceReviews
                .Include(r => r.Client)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return View();
        }

        // GET: Appointments - Lista administrativa (apenas para admins)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? status, int? professionalId, DateTime? startDate, DateTime? endDate)
        {
            var query = _appointmentRepository.GetAllWithIncludes().AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (professionalId.HasValue)
            {
                query = query.Where(a => a.ProfessionalId == professionalId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.StartTime <= endDate.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            // Get filter options
            ViewBag.Professionals = await _context.Professionals
                .Where(p => p.IsActive)
                .Select(p => new { p.ProfessionalId, p.Name })
                .ToListAsync();

            ViewBag.Statuses = new List<string> { "Pending", "Confirmed", "Completed", "Canceled" };

            return View(appointments);
        }

        // GET: Appointments/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name");
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty");
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name");
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create( Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();

                // Sincronizar com Google Calendar
                try
                {
                    await _calendarSyncService.CreateOrUpdateEventAsync(appointment);
                    _logger.LogInformation($"Evento Google Calendar criado para agendamento {appointment.AppointmentId}");
                }
                catch (Exception calendarEx)
                {
                    _logger.LogError(calendarEx, $"Erro ao sincronizar evento com Google Calendar para agendamento {appointment.AppointmentId}");
                }

                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name", appointment.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty", appointment.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name", appointment.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty", appointment.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,  Appointment appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();

                    // Sincronizar com Google Calendar
                    try
                    {
                        await _calendarSyncService.CreateOrUpdateEventAsync(appointment);
                        _logger.LogInformation($"Evento Google Calendar atualizado para agendamento {appointment.AppointmentId}");
                    }
                    catch (Exception calendarEx)
                    {
                        _logger.LogError(calendarEx, $"Erro ao sincronizar evento com Google Calendar para agendamento {appointment.AppointmentId}");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name", appointment.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty", appointment.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                // Remover evento do Google Calendar antes de deletar
                try
                {
                    if (!string.IsNullOrEmpty(appointment.GoogleEventId))
                    {
                        await _calendarSyncService.DeleteEventAsync(appointment.GoogleEventId);
                        _logger.LogInformation($"Evento Google Calendar removido para agendamento {appointment.AppointmentId}");
                    }
                }
                catch (Exception calendarEx)
                {
                    _logger.LogError(calendarEx, $"Erro ao remover evento do Google Calendar para agendamento {appointment.AppointmentId}");
                }

                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int professionalId, DateTime date)
        {
            try
            {
                Console.WriteLine($"=== BUSCANDO HORÁRIOS DISPONÍVEIS ===");
                Console.WriteLine($"ProfissionalId: {professionalId}, Data: {date:dd/MM/yyyy}");

                // 1. Verificar se o profissional existe e está ativo
                var professional = await _context.Professionals
                    .FirstOrDefaultAsync(p => p.ProfessionalId == professionalId && p.IsActive);

                if (professional == null)
                {
                    Console.WriteLine("❌ Profissional não encontrado ou inativo");
                    return Json(new { success = false, message = "Profissional não encontrado" });
                }

                // 2. Busca todos os horários do profissional para o dia da semana
                var schedules = await _context.ProfessionalSchedules
                    .Where(s => s.ProfessionalId == professionalId && s.DayOfWeek == date.DayOfWeek)
                    .ToListAsync();

                Console.WriteLine($"📅 Horários encontrados: {schedules.Count}");

                if (schedules == null || !schedules.Any())
                {
                    Console.WriteLine("❌ Nenhum horário configurado para este dia");
                    return Json(new { success = true, availableTimes = new List<string>(), message = "Nenhum horário disponível para este dia" });
                }

                // 3. Gera todos os slots possíveis (30 min) com base nos horários encontrados
                var allSlots = new List<string>();
                foreach (var schedule in schedules)
                {
                    Console.WriteLine($"⏰ Horário: {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm}");
                    for (var t = schedule.StartTime; t < schedule.EndTime; t = t.Add(TimeSpan.FromMinutes(30)))
                    {
                        allSlots.Add(t.ToString(@"hh\:mm"));
                    }
                }

                Console.WriteLine($"🕐 Total de slots gerados: {allSlots.Count}");

                // 4. Busca os horários já ocupados
                var bookedTimes = await _context.Appointments
                    .Where(a => a.ProfessionalId == professionalId && 
                               a.StartTime.Date == date.Date &&
                               a.Status != "Canceled")
                    .Select(a => a.StartTime.TimeOfDay)
                    .ToListAsync();

                Console.WriteLine($"📋 Horários ocupados: {bookedTimes.Count}");

                // 5. Remove os horários ocupados
                var availableTimes = allSlots
                    .Where(t => !bookedTimes.Any(bt => bt.ToString(@"hh\:mm") == t))
                    .ToList();

                Console.WriteLine($"✅ Horários disponíveis: {availableTimes.Count}");

                return Json(new { success = true, availableTimes = availableTimes });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO ao buscar horários: {ex.Message}");
                return Json(new { success = false, message = "Erro ao buscar horários disponíveis" });
            }
        }

        // POST: Admin/Appointments/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            var oldStatus = appointment.Status;
            appointment.Status = status;

            // Log the action
            var accessLog = new AccessLog
            {
                UserId = User.Identity.Name,
                Action = $"Updated appointment status",
                Timestamp = DateTime.Now,
                Details = $"Appointment {appointmentId}: {oldStatus} → {status}"
            };
            _context.AccessLogs.Add(accessLog);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Status updated successfully" });
        }

    }
}
