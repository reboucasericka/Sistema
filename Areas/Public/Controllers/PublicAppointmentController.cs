using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Sistema.Models.Public;
using Sistema.Services;
using Sistema.Models;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicAppointmentController : Controller
    {
        private readonly IApiServicesService _servicesService;
        private readonly IApiStaffService _staffService;
        private readonly IApiAppointmentsService _appointmentsService;
        private readonly ILogger<PublicAppointmentController> _logger;

        public PublicAppointmentController(
            IApiServicesService servicesService,
            IApiStaffService staffService,
            IApiAppointmentsService appointmentsService,
            ILogger<PublicAppointmentController> logger)
        {
            _servicesService = servicesService;
            _staffService = staffService;
            _appointmentsService = appointmentsService;
            _logger = logger;
        }

        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            try
            {
                var servicesResponse = await _servicesService.GetAllAsync();
                var staffResponse = await _staffService.GetAllAsync();

                var services = new List<ServiceDto>();
                var professionals = new List<ProfessionalDto>();

                if (servicesResponse.IsSuccess && servicesResponse.Data != null)
                {
                    services = servicesResponse.Data.Where(s => s.IsActive).ToList();
                }

                if (staffResponse.IsSuccess && staffResponse.Data != null)
                {
                    professionals = staffResponse.Data.Where(p => p.IsActive).ToList();
                }

                ViewBag.Services = services;
                ViewBag.Plans = new List<object>(); // Plans não estão disponíveis via API ainda
                ViewBag.Professionals = professionals;
                ViewBag.Reviews = new List<object>(); // Reviews não estão disponíveis via API ainda

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading appointment data");
                Console.WriteLine($"Erro ao carregar dados: {ex.Message}");
                
                ViewBag.Services = new List<ServiceDto>();
                ViewBag.Plans = new List<object>();
                ViewBag.Professionals = new List<ProfessionalDto>();
                ViewBag.Reviews = new List<object>();
                
                return View();
            }
        }

        // GET: Available times for a professional on a specific date
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int professionalId, DateTime date)
        {
            try
            {
                // Buscar agendamentos existentes para o profissional na data
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                var bookedTimes = new List<TimeSpan>();

                if (appointmentsResponse.IsSuccess && appointmentsResponse.Data != null)
                {
                    var dayAppointments = appointmentsResponse.Data
                        .Where(a => a.ProfessionalId == professionalId && 
                                   a.AppointmentDate.Date == date.Date)
                        .ToList();

                    foreach (var appointment in dayAppointments)
                    {
                        if (appointment.StartTime != default)
                        {
                            bookedTimes.Add(appointment.StartTime);
                        }
                    }
                }

                // Gerar horários disponíveis (9h às 18h, intervalos de 30 min)
                var availableTimes = new List<string>();
                var startTime = new TimeSpan(9, 0, 0);
                var endTime = new TimeSpan(18, 0, 0);
                var interval = new TimeSpan(0, 30, 0);

                for (var time = startTime; time < endTime; time = time.Add(interval))
                {
                    if (!bookedTimes.Contains(time))
                    {
                        availableTimes.Add(time.ToString(@"hh\:mm"));
                    }
                }

                return Json(availableTimes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available times for professional {Id} on {Date}", professionalId, date);
                return Json(new List<string>());
            }
        }

        // GET: Book appointment
        [HttpGet]
        public async Task<IActionResult> Book(int serviceId, int professionalId, DateTime date, string time)
        {
            try
            {
                // Buscar dados do serviço e profissional
                var serviceResponse = await _servicesService.GetByIdAsync(serviceId);
                var professionalResponse = await _staffService.GetByIdAsync(professionalId);

                if (!serviceResponse.IsSuccess || serviceResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Serviço não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                if (!professionalResponse.IsSuccess || professionalResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Profissional não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new PublicBookingViewModel
                {
                    ServiceId = serviceId,
                    ProfessionalId = professionalId,
                    SelectedDate = date,
                    SelectedTime = time,
                    ServiceName = serviceResponse.Data.Name,
                    ProfessionalName = professionalResponse.Data.Name
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking data");
                TempData["ErrorMessage"] = "An error occurred while loading booking data.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: My appointments
        [Authorize]
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

        // GET: Cancel appointment
        [Authorize]
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

        // GET: Reschedule appointment
        [Authorize]
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

                return View("Book", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading appointment for reschedule {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the appointment.";
                return RedirectToAction(nameof(MyAppointments));
            }
        }
    }
}