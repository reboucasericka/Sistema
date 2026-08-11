using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Admin;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly SignInManager<User> _signInManager;

        public AdminController(SistemaDbContext context, SignInManager<User> signInManager)
        {
            _context = context;
            _signInManager = signInManager;
        }

        // GET: Admin Dashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            // Monta o modelo tipado
            var model = new AdminDashboardViewModel
            {
                TotalAppointments = await _context.Appointments.CountAsync(),
                TotalClients = await _context.Customers.CountAsync(),
                TotalProfessionals = await _context.Professionals.CountAsync(),
                TotalServices = await _context.Services.CountAsync(),
                TodayAppointments = await _context.Appointments
                    .Where(a => a.StartTime.Date == DateTime.Today)
                    .CountAsync(),
                PendingAppointments = await _context.Appointments
                    .Where(a => a.StartTime >= DateTime.Today)
                    .CountAsync(),

                // Agendamentos de hoje
                TodayAppointmentsList = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Customer)
                    .Include(a => a.Professional)
                    .Include(a => a.Service)
                    .Where(a => a.StartTime.Date == DateTime.Today)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync(),

                // Próximos agendamentos
                UpcomingAppointmentsList = await _context.Appointments
                    .AsNoTracking()
                    .Include(a => a.Customer)
                    .Include(a => a.Professional)
                    .Include(a => a.Service)
                    .Where(a => a.StartTime > DateTime.Today)
                    .OrderBy(a => a.StartTime)
                    .Take(5)
                    .ToListAsync()
            };

            // 🔔 Preenche notificações
            var notifications = new List<AdminNotification>();

            // Exemplo: novos agendamentos
            var upcomingCount = model.UpcomingAppointmentsList.Count;
            if (upcomingCount > 0)
            {
                notifications.Add(new AdminNotification
                {
                    Message = $"{upcomingCount} novos agendamentos",
                    Icon = "fas fa-calendar-alt",
                    Time = "Hoje",
                    Link = "/Appointments"
                });
            }

            // Exemplo: clientes cadastrados
            var clientCount = await _context.Customers.CountAsync();
            notifications.Add(new AdminNotification
            {
                Message = $"{clientCount} clientes cadastrados",
                Icon = "fas fa-users",
                Time = DateTime.Now.ToString("HH:mm"),
                Link = "/Clients"
            });

            // Exemplo: serviços cadastrados
            var serviceCount = await _context.Services.CountAsync();
            notifications.Add(new AdminNotification
            {
                Message = $"{serviceCount} serviços disponíveis",
                Icon = "fas fa-concierge-bell",
                Time = DateTime.Now.ToString("HH:mm"),
                Link = "/Services"
            });

            // 🔹 NOVO → Agendamento daqui a 1 hora
            var oneHourFromNow = DateTime.Now.AddHours(1);
            var nextAppointment = await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .Where(a => a.StartTime.Date == DateTime.Today && a.StartTime.TimeOfDay >= DateTime.Now.TimeOfDay && a.StartTime.TimeOfDay <= oneHourFromNow.TimeOfDay)
                .OrderBy(a => a.StartTime)
                .FirstOrDefaultAsync();

            if (nextAppointment != null)
            {
                notifications.Add(new AdminNotification
                {
                    Message = $"Agendamento às {nextAppointment.Time:hh\\:mm} com {nextAppointment.Client?.Name}",
                    Icon = "fas fa-clock",
                    Time = "Em 1 hora",
                    Link = "/Appointments/Details/" + nextAppointment.AppointmentId
                });
            }

            // 🔹 NOVO → Profissionais sem disponibilidade hoje
            var today = DateTime.Today;
            var todayDayOfWeek = today.DayOfWeek;
            var busyProfessionals = await _context.Professionals
                .Where(p => !_context.ProfessionalSchedules.Any(s => s.ProfessionalId == p.ProfessionalId && s.DayOfWeek == todayDayOfWeek))
                .ToListAsync();

            if (busyProfessionals.Any())
            {
                var names = string.Join(", ", busyProfessionals.Select(p => p.Name));
                notifications.Add(new AdminNotification
                {
                    Message = $"Sem disponibilidade hoje: {names}",
                    Icon = "fas fa-user-times",
                    Time = today.ToString("dd/MM"),
                    Link = "/ProfessionalSchedules"
                });
            }

            

            // Passa para a ViewBag
            ViewBag.Notifications = notifications;

            return View(model);
        }

        // GET: Dados JSON para o gráfico de agendamentos
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointmentsChartData()
        {
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.StartTime >= startOfWeek)
                .ToListAsync();

            var data = appointments
                .GroupBy(a => a.StartTime.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToList();

            return Json(data);
        }
       
       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Notifications()
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

       

    }
}