using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using Sistema.Models.Public;
using Sistema.Services;
using Sistema.Models;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicAppointmentController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IGoogleCalendarSyncService _calendarSyncService;
        private readonly ILogger<PublicAppointmentController> _logger;

        public PublicAppointmentController(
            SistemaDbContext context, 
            IAppointmentRepository appointmentRepository, 
            ICustomerRepository customerRepository,
            IGoogleCalendarSyncService calendarSyncService,
            ILogger<PublicAppointmentController> logger)
        {
            _context = context;
            _appointmentRepository = appointmentRepository;
            _customerRepository = customerRepository;
            _calendarSyncService = calendarSyncService;
            _logger = logger;
        }

        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            try
            {
                var services = await _context.Services
                    .Include(s => s.Category)
                    .Where(s => s.IsActive)
                    .ToListAsync();

                var plans = await _context.Plans.ToListAsync();
                var professionals = await _context.Professionals
                    .Include(p => p.User)
                    .Where(p => p.IsActive)
                    .ToListAsync();
                var reviews = await _context.ServiceReviews
                    .Include(r => r.Client)
                    .OrderByDescending(r => r.ReviewDate)
                    .Take(10)
                    .ToListAsync();

                ViewBag.Services = services;
                ViewBag.Plans = plans;
                ViewBag.Professionals = professionals;
                ViewBag.Reviews = reviews;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar dados: {ex.Message}");
                
                ViewBag.Services = new List<Service>();
                ViewBag.Plans = new List<Plan>();
                ViewBag.Professionals = new List<Professional>();
                ViewBag.Reviews = new List<ServiceReview>();
                
                return View();
            }
        }

        // GET: Available times for a professional on a specific date
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int professionalId, DateTime date)
        {
            try
            {
                // Verificar se o profissional existe e está ativo
                var professional = await _context.Professionals
                    .FirstOrDefaultAsync(p => p.ProfessionalId == professionalId && p.IsActive);

                if (professional == null)
                {
                    return Json(new { success = false, message = "Profissional não encontrado" });
                }

                // Buscar horários configurados para o dia da semana
                var schedules = await _context.ProfessionalSchedules
                    .Where(s => s.ProfessionalId == professionalId && s.DayOfWeek == date.DayOfWeek)
                    .ToListAsync();

                if (!schedules.Any())
                {
                    return Json(new { success = true, availableTimes = new List<string>(), message = "Nenhum horário disponível para este dia" });
                }

                // Gerar slots de 30 minutos
                var allSlots = new List<string>();
                foreach (var schedule in schedules)
                {
                    for (var t = schedule.StartTime; t < schedule.EndTime; t = t.Add(TimeSpan.FromMinutes(30)))
                    {
                        allSlots.Add(t.ToString(@"hh\:mm"));
                    }
                }

                // Buscar horários já ocupados
                var bookedTimes = await _context.Appointments
                    .Where(a => a.ProfessionalId == professionalId && 
                               a.StartTime.Date == date.Date &&
                               a.Status != "Canceled")
                    .Select(a => a.StartTime.TimeOfDay)
                    .ToListAsync();

                // Remover horários ocupados
                var availableTimes = allSlots
                    .Where(t => !bookedTimes.Any(bt => bt.ToString(@"hh\:mm") == t))
                    .ToList();

                return Json(new { success = true, availableTimes = availableTimes });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar horários: {ex.Message}");
                return Json(new { success = false, message = "Erro ao buscar horários disponíveis" });
            }
        }

        // POST: Create appointment
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, message = "Dados inválidos" });
                }

                // Verificar se o horário está disponível
                var startTime = request.Date.Date.Add(request.Time);
                var endTime = startTime.AddMinutes(request.Duration);

                var isAvailable = await _appointmentRepository.IsTimeSlotAvailableAsync(
                    request.ProfessionalId, startTime, endTime);

                if (!isAvailable)
                {
                    return Json(new { success = false, message = "Horário não disponível" });
                }

                // Buscar ou criar cliente
                var customer = await _customerRepository.GetByUserId(User.Identity.Name).FirstOrDefaultAsync();
                if (customer == null)
                {
                    return Json(new { success = false, message = "Cliente não encontrado. Faça login novamente." });
                }

                // Criar agendamento
                var appointment = new Appointment
                {
                    CustomerId = customer.CustomerId,
                    ProfessionalId = request.ProfessionalId,
                    ServiceId = request.ServiceId,
                    StartTime = startTime,
                    EndTime = endTime,
                    Status = "Pending",
                    Notes = request.Notes,
                    TotalPrice = request.Price
                };

                await _appointmentRepository.CreateAsync(appointment);

                return Json(new { success = true, message = "Agendamento criado com sucesso!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar agendamento: {ex.Message}");
                return Json(new { success = false, message = "Erro ao criar agendamento" });
            }
        }

    }
}