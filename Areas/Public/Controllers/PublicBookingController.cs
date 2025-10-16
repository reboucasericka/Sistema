using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Public;
using System.Security.Claims;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize(Roles = "Customer")]
    public class PublicBookingController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicBookingController(SistemaDbContext context)
        {
            _context = context;
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
                    
                    var appointment = new Appointment
                    {
                        CustomerId = int.Parse(userId),
                        ServiceId = model.ServiceId,
                        StartTime = startTime,
                        EndTime = endTime,
                        Status = "Scheduled"
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    // Log access
                    await LogAccess("BOOKING", $"Service booked: {model.ServiceName} for {model.SelectedDate:dd/MM/yyyy} at {model.SelectedTime}");

                    TempData["Message"] = "Appointment scheduled successfully!";
                    return RedirectToAction(nameof(MyAppointments));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error scheduling appointment. Please try again.";
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
                TempData["Message"] = "Agendamento cancelado com sucesso!";
            }

            return RedirectToAction(nameof(MyAppointments));
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