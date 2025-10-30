using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
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
        private readonly IApiServicesService _servicesService;
        private readonly IApiStaffService _staffService;
        private readonly IApiAppointmentsService _appointmentsService;
        private readonly IApiClientService _apiClient;
        private readonly IAppointmentNotificationService _notificationService;
        private readonly ILogger<PublicBookingController> _logger;

        public PublicBookingController(
            IApiServicesService servicesService,
            IApiStaffService staffService,
            IApiAppointmentsService appointmentsService,
            IAppointmentNotificationService notificationService,
            ILogger<PublicBookingController> logger,
            IApiClientService apiClient)
        {
            _servicesService = servicesService;
            _staffService = staffService;
            _appointmentsService = appointmentsService;
            _notificationService = notificationService;
            _logger = logger;
            _apiClient = apiClient;
        }

        // GET: Public/Booking
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _servicesService.GetAllAsync();
                
                if (response.IsSuccess && response.Data != null)
                {
                    var services = response.Data.Where(s => s.IsActive).ToList();
            return View(services);
                }
                else
                {
                    _logger.LogError("Failed to fetch services: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Failed to load services. Please try again.";
                    return View(new List<ServiceDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching services");
                TempData["ErrorMessage"] = "An error occurred while loading services.";
                return View(new List<ServiceDto>());
            }
        }

        // GET: Public/Booking/Schedule/5
        public async Task<IActionResult> Schedule(int id)
        {
            // Novo fluxo: retornar ServiceDto para view de seleção de horário/profissional
            var apiResp = await _apiClient.GetAsync<SistemaAPI.DTOs.ServiceDto>("services/" + id);
            if (apiResp?.Data == null)
            {
                TempData["ErrorMessage"] = "Serviço não encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(apiResp.Data);
        }

        // POST: Public/Booking/Schedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(PublicBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var serviceResponse = await _servicesService.GetByIdAsync(model.ServiceId);
                    
                    if (!serviceResponse.IsSuccess || serviceResponse.Data == null)
                {
                    return NotFound();
                }

                    var service = serviceResponse.Data;

                model.ServiceName = service.Name;
                model.Price = service.Price;
                model.Duration = service.Duration;
                model.Description = service.Description;
                model.ImageId = service.ImageId.ToString();

                return RedirectToAction(nameof(Confirm), model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading service for booking confirmation");
                    TempData["ErrorMessage"] = "An error occurred while processing your booking.";
                }
            }

            // Se houver erro, recarregar os dados necessários
            try
            {
                var serviceResponse = await _servicesService.GetByIdAsync(model.ServiceId);
                
                if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                // Buscar profissionais que oferecem este serviço
                    var staffResponse = await _staffService.GetAllAsync();
                    var professionals = new List<ProfessionalDto>();
                    
                    if (staffResponse.IsSuccess && staffResponse.Data != null)
                    {
                        professionals = staffResponse.Data.Where(p => p.IsActive).ToList();
                    }

                ViewBag.AvailableTimes = GenerateAvailableTimes();
                ViewBag.Professionals = professionals;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading service data for booking");
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
                    // Buscar dados do serviço para obter informações completas
                    var serviceResponse = await _servicesService.GetByIdAsync(model.ServiceId);
                    if (!serviceResponse.IsSuccess || serviceResponse.Data == null)
                    {
                        TempData["ErrorMessage"] = "Serviço não encontrado.";
                        return View("Confirm", model);
                    }
                    
                    // Criar o agendamento via API
                    var appointmentDto = new AppointmentDto
                    {
                        ServiceId = model.ServiceId,
                        ProfessionalId = model.ProfessionalId,
                        ClientName = model.ServiceName, // Usar ServiceName como fallback
                        ClientEmail = User.Identity?.Name ?? "",
                        AppointmentDate = model.SelectedDate,
                        StartTime = TimeSpan.Parse(model.SelectedTime),
                        EndTime = TimeSpan.Parse(model.SelectedTime).Add(TimeSpan.FromHours(1)), // Assumir 1 hora de duração
                        Notes = model.Description,
                        Status = "Agendado"
                    };

                    var createResponse = await _appointmentsService.CreateAsync(appointmentDto);
                    
                    if (createResponse.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Agendamento confirmado com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao confirmar o agendamento. Tente novamente.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error confirming booking");
                    TempData["ErrorMessage"] = "An error occurred while confirming your booking.";
                }
            }

            return View("Confirm", model);
        }

        // GET: Public/Booking/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            try
            {
                // Buscar agendamentos do usuário logado
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var userAppointments = new List<AppointmentDto>();

                if (appointmentsResponse.IsSuccess && appointmentsResponse.Data != null)
                {
                    var userEmail = User.Identity?.Name;
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        userAppointments = appointmentsResponse.Data
                            .Where(a => a.ClientEmail == userEmail)
                            .OrderByDescending(a => a.AppointmentDate)
                            .ThenBy(a => a.StartTime)
                            .ToList();
                    }
                }

                return View(userAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user appointments");
                TempData["ErrorMessage"] = "An error occurred while loading your appointments.";
                return View(new List<AppointmentDto>());
            }
        }

        // GET: Public/Booking/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                // Buscar o agendamento para verificar se pertence ao usuário
                var appointmentResponse = await _appointmentsService.GetByIdAsync(id);
                
                if (!appointmentResponse.IsSuccess || appointmentResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Agendamento não encontrado.";
                    return RedirectToAction(nameof(MyAppointments));
                }

                var userEmail = User.Identity?.Name;
                if (appointmentResponse.Data.ClientEmail != userEmail)
                {
                    TempData["ErrorMessage"] = "Você não tem permissão para cancelar este agendamento.";
                    return RedirectToAction(nameof(MyAppointments));
                }

                // Cancelar o agendamento via API
                var cancelResponse = await _appointmentsService.DeleteAsync(id);
                
                if (cancelResponse.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Agendamento cancelado com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao cancelar o agendamento. Tente novamente.";
                }

                return RedirectToAction(nameof(MyAppointments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling appointment {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while canceling the appointment.";
                return RedirectToAction(nameof(MyAppointments));
            }
        }

        // GET: Public/Booking/Reschedule/5
        public async Task<IActionResult> Reschedule(int id)
        {
            try
            {
                // Buscar o agendamento para verificar se pertence ao usuário
                var appointmentResponse = await _appointmentsService.GetByIdAsync(id);
                
                if (!appointmentResponse.IsSuccess || appointmentResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Agendamento não encontrado.";
                    return RedirectToAction(nameof(MyAppointments));
                }

                var userEmail = User.Identity?.Name;
                if (appointmentResponse.Data.ClientEmail != userEmail)
                {
                    TempData["ErrorMessage"] = "Você não tem permissão para reagendar este agendamento.";
                    return RedirectToAction(nameof(MyAppointments));
                }

                // Buscar dados do serviço e profissional para o reagendamento
                var serviceResponse = await _servicesService.GetByIdAsync(appointmentResponse.Data.ServiceId);
                var professionalResponse = await _staffService.GetByIdAsync(appointmentResponse.Data.ProfessionalId);

                if (!serviceResponse.IsSuccess || !professionalResponse.IsSuccess)
                {
                    TempData["ErrorMessage"] = "Erro ao carregar dados para reagendamento.";
                    return RedirectToAction(nameof(MyAppointments));
                }

                var model = new PublicBookingViewModel
                {
                    ServiceId = appointmentResponse.Data.ServiceId,
                    ProfessionalId = appointmentResponse.Data.ProfessionalId,
                    SelectedDate = appointmentResponse.Data.AppointmentDate,
                    SelectedTime = appointmentResponse.Data.StartTime.ToString(@"hh\:mm"),
                    ServiceName = serviceResponse.Data.Name,
                    ProfessionalName = professionalResponse.Data.Name
                };

                return View("Schedule", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading appointment for reschedule {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the appointment.";
                return RedirectToAction(nameof(MyAppointments));
            }
        }

        // Método auxiliar para gerar horários disponíveis
        private List<string> GenerateAvailableTimes()
        {
            var times = new List<string>();
            var startTime = new TimeSpan(9, 0, 0); // 9:00
            var endTime = new TimeSpan(18, 0, 0);   // 18:00
            var interval = new TimeSpan(0, 30, 0);  // 30 minutos

            for (var time = startTime; time < endTime; time = time.Add(interval))
            {
                times.Add(time.ToString(@"hh\:mm"));
            }

            return times;
        }

        // ===== Novas actions para fluxo elegante =====
        [HttpGet]
        public async Task<IActionResult> _DaySlots(int serviceId, DateOnly date)
        {
            var resp = await _apiClient.GetAsync<List<SistemaAPI.DTOs.AvailableSlotDto>>($"appointments/available?serviceId={serviceId}&date={date:yyyy-MM-dd}");
            var slots = resp?.Data ?? new List<SistemaAPI.DTOs.AvailableSlotDto>();
            return PartialView("_DaySlots", slots);
        }

        [HttpGet]
        public async Task<IActionResult> _StaffForSlot(int serviceId, string time, DateOnly date)
        {
            var resp = await _apiClient.GetAsync<List<SistemaAPI.DTOs.ProfessionalAvailabilityDto>>($"professionals/available?serviceId={serviceId}&date={date:yyyy-MM-dd}&time={time}");
            var staff = resp?.Data ?? new List<SistemaAPI.DTOs.ProfessionalAvailabilityDto>();
            return PartialView("_StaffForSlot", staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm([FromForm] SistemaAPI.DTOs.CreateAppointmentDto dto)
        {
            var created = await _apiClient.PostAsync<SistemaAPI.DTOs.AppointmentDto>("appointments", dto);
            if (created?.Data == null)
            {
                TempData["ErrorMessage"] = "Falha ao agendar.";
                return BadRequest("Falha ao agendar.");
            }

            _ = await _apiClient.PostAsync<object>($"appointments/{created.Data.Id}/sync-google", new { });
            TempData["SuccessMessage"] = "Agendamento confirmado!";
            return RedirectToAction("MyAppointments", "PublicClientPanel", new { area = "Public" });
        }
    }
}