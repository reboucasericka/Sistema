using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Microsoft.Extensions.Logging;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApiAppointmentsService _appointmentsService;
        private readonly ApiClientsService _clientsService;
        private readonly ApiStaffService _staffService;
        private readonly ApiServicesService _servicesService;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ApiAppointmentsService appointmentsService,
            ApiClientsService clientsService,
            ApiStaffService staffService,
            ApiServicesService servicesService,
            SignInManager<User> signInManager,
            ILogger<AdminController> logger)
        {
            _appointmentsService = appointmentsService;
            _clientsService = clientsService;
            _staffService = staffService;
            _servicesService = servicesService;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: Admin Dashboard
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            try
            {
                // Buscar dados via API
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var clientsResponse = await _clientsService.GetAllAsync();
                var staffResponse = await _staffService.GetAllAsync();
                var servicesResponse = await _servicesService.GetAllAsync();

                // Calcular estatísticas
                var appointments = appointmentsResponse.Success ? appointmentsResponse.Data ?? new List<AppointmentDto>() : new List<AppointmentDto>();
                var clients = clientsResponse.Success ? clientsResponse.Data ?? new List<ClientDto>() : new List<ClientDto>();
                var staff = staffResponse.Success ? staffResponse.Data ?? new List<ProfessionalDto>() : new List<ProfessionalDto>();
                var services = servicesResponse.Success ? servicesResponse.Data ?? new List<ServiceDto>() : new List<ServiceDto>();

                // Monta o modelo tipado
                var model = new AdminDashboardViewModel
                {
                    TotalAppointments = appointments.Count(),
                    TotalClients = clients.Count(),
                    TotalProfessionals = staff.Count(),
                    TotalServices = services.Count(),
                    TodayAppointments = appointments.Count(a => a.AppointmentDate.Date == DateTime.Today),
                    PendingAppointments = appointments.Count(a => a.AppointmentDate >= DateTime.Today),

                    // Agendamentos de hoje
                    TodayAppointmentsList = appointments
                        .Where(a => a.AppointmentDate.Date == DateTime.Today)
                        .OrderBy(a => a.AppointmentDate)
                        .Select(a => new Appointment 
                        { 
                            AppointmentId = a.AppointmentId, 
                            StartTime = a.AppointmentDate.Add(a.StartTime), 
                            EndTime = a.AppointmentDate.Add(a.EndTime), 
                            Notes = a.Notes, 
                            Status = a.Status, 
                            CustomerId = a.ClientId, 
                            ServiceId = a.ServiceId, 
                            ProfessionalId = a.ProfessionalId 
                        })
                        .ToList(),

                    // Próximos agendamentos
                    UpcomingAppointmentsList = appointments
                        .Where(a => a.AppointmentDate > DateTime.Today)
                        .OrderBy(a => a.AppointmentDate)
                        .Take(5)
                        .Select(a => new Appointment 
                        { 
                            AppointmentId = a.AppointmentId, 
                            StartTime = a.AppointmentDate.Add(a.StartTime), 
                            EndTime = a.AppointmentDate.Add(a.EndTime), 
                            Notes = a.Notes, 
                            Status = a.Status, 
                            CustomerId = a.ClientId, 
                            ServiceId = a.ServiceId, 
                            ProfessionalId = a.ProfessionalId 
                        })
                        .ToList()
                };

                // 🔔 Preenche notificações
                var notifications = new List<AdminNotification>();

                // Exemplo: novos agendamentos
                var upcomingCount = appointments.Count(a => a.AppointmentDate > DateTime.Today);
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
                notifications.Add(new AdminNotification
                {
                    Message = $"{clients.Count()} clientes cadastrados",
                    Icon = "fas fa-users",
                    Time = DateTime.Now.ToString("HH:mm"),
                    Link = "/Clients"
                });

                // Exemplo: serviços cadastrados
                notifications.Add(new AdminNotification
                {
                    Message = $"{services.Count()} serviços disponíveis",
                    Icon = "fas fa-concierge-bell",
                    Time = DateTime.Now.ToString("HH:mm"),
                    Link = "/Services"
                });

                // 🔹 NOVO → Agendamento daqui a 1 hora
                var oneHourFromNow = DateTime.Now.AddHours(1);
                var nextAppointment = appointments
                    .Where(a => a.AppointmentDate.Date == DateTime.Today && 
                               a.AppointmentDate.TimeOfDay >= DateTime.Now.TimeOfDay && 
                               a.AppointmentDate.TimeOfDay <= oneHourFromNow.TimeOfDay)
                    .OrderBy(a => a.AppointmentDate)
                    .FirstOrDefault();

                if (nextAppointment != null)
                {
                    notifications.Add(new AdminNotification
                    {
                        Message = $"Agendamento às {nextAppointment.AppointmentDate.Add(nextAppointment.StartTime):HH\\:mm}",
                        Icon = "fas fa-clock",
                        Time = "Em 1 hora",
                        Link = "/Appointments/Details/" + nextAppointment.AppointmentId
                    });
                }

                // Passa para a ViewBag
                ViewBag.Notifications = notifications;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard data");
                TempData["ErrorMessage"] = "An error occurred while loading dashboard data.";
                
                // Retornar modelo vazio em caso de erro
                return View(new AdminDashboardViewModel
                {
                    TotalAppointments = 0,
                    TotalClients = 0,
                    TotalProfessionals = 0,
                    TotalServices = 0,
                    TodayAppointments = 0,
                    PendingAppointments = 0,
                    TodayAppointmentsList = new List<Appointment>(),
                    UpcomingAppointmentsList = new List<Appointment>()
                });
            }
        }

        // GET: Dados JSON para o gráfico de agendamentos
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAppointmentsChartData()
        {
            try
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var appointments = appointmentsResponse.Success ? 
                    appointmentsResponse.Data?.Where(a => a.AppointmentDate >= startOfWeek).ToList() ?? new List<AppointmentDto>() : 
                    new List<AppointmentDto>();

                var data = appointments
                    .GroupBy(a => a.AppointmentDate.DayOfWeek)
                    .Select(g => new
                    {
                        Day = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chart data");
                return Json(new List<object>());
            }
        }
       
       
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Notifications()
        {
            try
            {
                // Implementar busca de notificações via API quando disponível
                var notifications = new List<object>();
                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading notifications");
                TempData["ErrorMessage"] = "An error occurred while loading notifications.";
                return View(new List<object>());
            }
        }

       

    }
}