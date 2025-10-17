using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Public;
using Sistema.Models;
using Sistema.Services;
using System.Security.Claims;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize(Roles = "Customer")]
    public class PublicBookingController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IAppointmentNotificationService _notificationService;
        private readonly INotificationService _emailNotificationService;
        private readonly IGoogleCalendarSyncService _calendarSyncService;
        private readonly ILogger<PublicBookingController> _logger;

        public PublicBookingController(
            SistemaDbContext context, 
            IAppointmentNotificationService notificationService,
            INotificationService emailNotificationService,
            IGoogleCalendarSyncService calendarSyncService,
            ILogger<PublicBookingController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _emailNotificationService = emailNotificationService;
            _calendarSyncService = calendarSyncService;
            _logger = logger;
        }

        // GET: Public/Booking
        public async Task<IActionResult> Index()
        {
                    var services = await _context.Services
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category.Name)
                .ThenBy(s => s.Name)
                .ToListAsync();

            return View(services);
        }

        // GET: Public/Booking/Schedule/5
        public async Task<IActionResult> Schedule(int id)
        {
            var service = await _context.Services
                .Include(s => s.Category)
                .Include(s => s.ProfessionalServices)
                    .ThenInclude(ps => ps.Professional)
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.IsActive);

            if (service == null)
            {
                return NotFound();
            }

            var viewModel = new PublicBookingViewModel
            {
                ServiceId = service.ServiceId,
                ServiceName = service.Name,
                Price = service.Price,
                Duration = service.Duration,
                Description = service.Description,
                ImageId = service.ImageId.ToString(),
                SelectedDate = DateTime.Now.Date
            };

            // Gerar horários disponíveis (exemplo: 9h às 18h, intervalos de 30 min)
            ViewBag.AvailableTimes = GenerateAvailableTimes();
            ViewBag.Professionals = service.ProfessionalServices
                .Select(ps => ps.Professional)
                .ToList();

            return View(viewModel);
        }

        // POST: Public/Booking/Schedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(PublicBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Aqui você implementaria a lógica de criação do agendamento
                // Por enquanto, vamos apenas redirecionar para uma página de confirmação
                
                var service = await _context.Services
                    .Include(s => s.Category)
                    .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId);

                if (service == null)
                {
                    return NotFound();
                }

                model.ServiceName = service.Name;
                model.Price = service.Price;
                model.Duration = service.Duration;
                model.Description = service.Description;
                model.ImageId = service.ImageId.ToString();

                return RedirectToAction(nameof(Confirm), model);
            }

            // Se houver erro, recarregar os dados necessários
            var serviceForView = await _context.Services
                .Include(s => s.Category)
                .Include(s => s.ProfessionalServices)
                    .ThenInclude(ps => ps.Professional)
                .FirstOrDefaultAsync(s => s.ServiceId == model.ServiceId);

            if (serviceForView != null)
            {
                ViewBag.AvailableTimes = GenerateAvailableTimes();
                ViewBag.Professionals = serviceForView.ProfessionalServices
                    .Select(ps => ps.Professional)
                    .ToList();
            }

            return View(model);
        }

        // GET: Public/Booking/Confirm
        public IActionResult Confirm(PublicBookingViewModel model)
        {
            return View(model);
        }

        // POST: Public/Booking/Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(PublicBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obter o ID do usuário logado
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (string.IsNullOrEmpty(userId))
                    {
                        TempData["Error"] = "User not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Criar o agendamento
                    var startTime = model.SelectedDate.Add(TimeSpan.Parse(model.SelectedTime));
                    var service = await _context.Services.FindAsync(model.ServiceId);
                    var endTime = startTime.AddMinutes(service?.DurationInMinutes ?? 60);

                    // VALIDAÇÃO ANTI-CONCORRÊNCIA: Verificar se o horário ainda está disponível
                    var conflictingAppointment = await _context.Appointments
                        .FirstOrDefaultAsync(a => a.ProfessionalId == model.ProfessionalId &&
                                                 a.StartTime.Date == startTime.Date &&
                                                 a.Status != "Cancelado" &&
                                                 (startTime < a.EndTime && endTime > a.StartTime));

                    if (conflictingAppointment != null)
                    {
                        TempData["Error"] = "Horário já ocupado. Por favor, selecione outro horário.";
                        return View("Confirm", model);
                    }
                    
                    var appointment = new Appointment
                    {
                        CustomerId = int.Parse(userId),
                        ServiceId = model.ServiceId,
                        ProfessionalId = model.ProfessionalId,
                        StartTime = startTime,
                        EndTime = endTime,
                        Status = "Agendado",
                        TotalPrice = service?.Price
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    // Carregar as entidades relacionadas para o e-mail
                    await _context.Entry(appointment)
                        .Reference(a => a.Customer)
                        .LoadAsync();
                    await _context.Entry(appointment)
                        .Reference(a => a.Service)
                        .LoadAsync();
                    await _context.Entry(appointment)
                        .Reference(a => a.Professional)
                        .LoadAsync();

                    // Enviar notificações de confirmação
                    await _notificationService.SendAppointmentConfirmationAsync(appointment);

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

                    // Enviar e-mail de confirmação
                    try
                    {
                        await _emailNotificationService.SendBookingConfirmationAsync(appointment);
                    }
                    catch (Exception emailEx)
                    {
                        // Log do erro mas não falha o agendamento
                        // O agendamento já foi salvo com sucesso
                        TempData["Warning"] = "Agendamento criado com sucesso, mas houve um problema ao enviar o e-mail de confirmação.";
                    }

                    // Log access
                    await LogAccess("BOOKING", $"Service booked: {model.ServiceName} for {model.SelectedDate:dd/MM/yyyy} at {model.SelectedTime}");

                    TempData["Message"] = "Agendamento realizado com sucesso!";
                    return RedirectToAction(nameof(MyAppointments));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Erro ao agendar. Tente novamente.";
                    return View("Confirm", model);
                }
            }

            return View("Confirm", model);
        }

        // GET: Public/Booking/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var appointments = await _context.Appointments
                .Include(a => a.Service)
                    .ThenInclude(s => s.Category)
                .Include(a => a.Customer)
                .Where(a => a.CustomerId == int.Parse(userId))
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToListAsync();

            return View(appointments);
        }

        // GET: Public/Booking/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.CustomerId == int.Parse(userId));

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Public/Booking/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.CustomerId == int.Parse(userId));

            if (appointment != null)
            {
                appointment.Status = "Cancelado";
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                // Carregar as entidades relacionadas para o e-mail
                await _context.Entry(appointment)
                    .Reference(a => a.Customer)
                    .LoadAsync();
                await _context.Entry(appointment)
                    .Reference(a => a.Service)
                    .LoadAsync();
                await _context.Entry(appointment)
                    .Reference(a => a.Professional)
                    .LoadAsync();

                // Remover evento do Google Calendar
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

                // Enviar notificações de cancelamento
                await _notificationService.SendAppointmentCancellationAsync(appointment);

                // Enviar e-mail de cancelamento
                try
                {
                    await _emailNotificationService.SendBookingCancelledAsync(appointment);
                }
                catch (Exception emailEx)
                {
                    // Log do erro mas não falha o cancelamento
                    // O cancelamento já foi salvo com sucesso
                    TempData["Warning"] = "Agendamento cancelado com sucesso, mas houve um problema ao enviar o e-mail de confirmação.";
                }

                TempData["Message"] = "Agendamento cancelado com sucesso!";
            }

            return RedirectToAction(nameof(MyAppointments));
        }

        // POST: Public/Booking/Reschedule/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime newStartTime, int newProfessionalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.CustomerId == int.Parse(userId));

            if (appointment != null)
            {
                // Salvar horário anterior para referência
                var oldStartTime = appointment.StartTime;
                var oldProfessionalId = appointment.ProfessionalId;

                // Atualizar agendamento
                appointment.StartTime = newStartTime;
                appointment.ProfessionalId = newProfessionalId;
                appointment.EndTime = newStartTime.AddMinutes(appointment.Service.DurationInMinutes);
                
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                // Carregar as entidades relacionadas para o e-mail
                await _context.Entry(appointment)
                    .Reference(a => a.Customer)
                    .LoadAsync();
                await _context.Entry(appointment)
                    .Reference(a => a.Service)
                    .LoadAsync();
                await _context.Entry(appointment)
                    .Reference(a => a.Professional)
                    .LoadAsync();

                // Enviar e-mail de reagendamento
                try
                {
                    await _emailNotificationService.SendBookingRescheduledAsync(appointment);
                }
                catch (Exception emailEx)
                {
                    // Log do erro mas não falha o reagendamento
                    // O reagendamento já foi salvo com sucesso
                    TempData["Warning"] = "Agendamento reagendado com sucesso, mas houve um problema ao enviar o e-mail de confirmação.";
                }

                TempData["Message"] = "Agendamento reagendado com sucesso!";
            }

            return RedirectToAction(nameof(MyAppointments));
        }

        // GET: Public/Booking/GetAvailableSlots
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int professionalId, int serviceId, DateTime date)
        {
            try
            {
                // Buscar o serviço para obter a duração
                var service = await _context.Services.FindAsync(serviceId);
                if (service == null)
                {
                    return Json(new { success = false, message = "Serviço não encontrado" });
                }

                // Buscar agendamentos existentes para o profissional na data
                var existingAppointments = await _context.Appointments
                    .Where(a => a.ProfessionalId == professionalId && 
                               a.StartTime.Date == date.Date &&
                               a.Status != "Cancelado")
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                // Gerar horários disponíveis (9h às 18h, intervalos de 30 min)
                var availableSlots = new List<object>();
                var startTime = new TimeSpan(9, 0, 0); // 9:00
                var endTime = new TimeSpan(18, 0, 0);   // 18:00
                var interval = new TimeSpan(0, 30, 0);  // 30 minutos

                for (var time = startTime; time <= endTime; time = time.Add(interval))
                {
                    var slotStart = date.Date.Add(time);
                    var slotEnd = slotStart.AddMinutes(service.DurationInMinutes);

                    // Verificar se o horário não conflita com agendamentos existentes
                    var isAvailable = !existingAppointments.Any(a => 
                        (slotStart < a.EndTime && slotEnd > a.StartTime));

                    if (isAvailable)
                    {
                        availableSlots.Add(new
                        {
                            time = time.ToString(@"hh\:mm"),
                            start = slotStart.ToString("yyyy-MM-ddTHH:mm:ss"),
                            end = slotEnd.ToString("yyyy-MM-ddTHH:mm:ss"),
                            duration = service.DurationInMinutes
                        });
                    }
                }

                return Json(new { success = true, slots = availableSlots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao buscar horários disponíveis" });
            }
        }

        private List<string> GenerateAvailableTimes()
        {
            var times = new List<string>();
            var startTime = new TimeSpan(9, 0, 0); // 9:00
            var endTime = new TimeSpan(18, 0, 0);   // 18:00
            var interval = new TimeSpan(0, 30, 0);  // 30 minutos

            for (var time = startTime; time <= endTime; time = time.Add(interval))
            {
                times.Add(time.ToString(@"hh\:mm"));
            }

            return times;
        }

        private async Task LogAccess(string action, string details)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var accessLog = new AccessLog
                {
                    UserId = userId,
                    Action = action,
                    Details = details,
                    Timestamp = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                _context.AccessLogs.Add(accessLog);
                await _context.SaveChangesAsync();
            }
        }
    }
}