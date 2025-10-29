using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Microsoft.Extensions.Logging;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminAppointmentsController : Controller
    {
        private readonly ApiAppointmentsService _appointmentsService;
        private readonly ApiClientsService _clientsService;
        private readonly ApiServicesService _servicesService;
        private readonly ApiStaffService _staffService;
        private readonly ILogger<AdminAppointmentsController> _logger;

        public AdminAppointmentsController(
            ApiAppointmentsService appointmentsService,
            ApiClientsService clientsService,
            ApiServicesService servicesService,
            ApiStaffService staffService,
            ILogger<AdminAppointmentsController> logger)
        {
            _appointmentsService = appointmentsService;
            _clientsService = clientsService;
            _servicesService = servicesService;
            _staffService = staffService;
            _logger = logger;
        }


        // ? P�gina p�blica de agendamento online
        [AllowAnonymous] // N�o pede login
        public async Task<IActionResult> Public()
        {
            ViewData["Title"] = "Agendamento Online";

            try
            {
                // Carregar os dados necess�rios via API
                var servicesResponse = await _servicesService.GetActiveAsync();
                var staffResponse = await _staffService.GetActiveAsync();

                ViewBag.Services = servicesResponse.Success ? servicesResponse.Data ?? new List<ServiceDto>() : new List<ServiceDto>();
                ViewBag.Professionals = staffResponse.Success ? staffResponse.Data ?? new List<ProfessionalDto>() : new List<ProfessionalDto>();
                
                // Para plans e reviews, vamos usar dados vazios por enquanto
                // (estes endpoints precisam ser criados na API)
                ViewBag.Plans = new List<object>();
                ViewBag.Reviews = new List<object>();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading public appointment data");
                ViewBag.Services = new List<ServiceDto>();
                ViewBag.Professionals = new List<ProfessionalDto>();
                ViewBag.Plans = new List<object>();
                ViewBag.Reviews = new List<object>();
                return View();
            }
        }

        // GET: Appointments - Lista administrativa (apenas para admins)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? status, int? professionalId, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Buscar todos os agendamentos via API
                var appointmentsResponse = await _appointmentsService.GetAllAsync();
                
                if (!appointmentsResponse.Success || appointmentsResponse.Data == null)
                {
                    _logger.LogError("Failed to fetch appointments: {Message}", appointmentsResponse.Message);
                    TempData["ErrorMessage"] = "Failed to load appointments. Please try again.";
                    return View(new List<AppointmentDto>());
                }

                var appointments = appointmentsResponse.Data.ToList();

                // Aplicar filtros (por enquanto no MVC, mas idealmente deveria ser na API)
                if (!string.IsNullOrEmpty(status))
                {
                    appointments = appointments.Where(a => a.Status == status).ToList();
                }

                if (professionalId.HasValue)
                {
                    appointments = appointments.Where(a => a.ProfessionalId == professionalId.Value).ToList();
                }

                if (startDate.HasValue)
                {
                    appointments = appointments.Where(a => a.AppointmentDate >= startDate.Value).ToList();
                }

                if (endDate.HasValue)
                {
                    appointments = appointments.Where(a => a.AppointmentDate <= endDate.Value).ToList();
                }

                // Ordenar por data
                appointments = appointments.OrderByDescending(a => a.AppointmentDate).ToList();

                // Buscar op��es de filtro via API
                var staffResponse = await _staffService.GetActiveAsync();
                if (staffResponse.Success && staffResponse.Data != null)
                {
                    ViewBag.Professionals = staffResponse.Data.Select(p => new { ProfessionalId = p.ProfessionalId, Name = p.Name }).ToList();
                }
                else
                {
                    ViewBag.Professionals = new List<object>();
                }

                ViewBag.Statuses = new List<string> { "Pending", "Confirmed", "Completed", "Canceled" };

                return View(appointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading appointments");
                TempData["ErrorMessage"] = "An error occurred while loading appointments.";
                return View(new List<AppointmentDto>());
            }
        }

        // GET: Appointments/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _appointmentsService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch appointment {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment {Id}", id);
                return NotFound();
            }
        }

        // GET: Appointments/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Buscar dados para os dropdowns via API
                var clientsResponse = await _clientsService.GetAllAsync();
                var staffResponse = await _staffService.GetAllAsync();
                var servicesResponse = await _servicesService.GetAllAsync();

                ViewData["CustomerId"] = new SelectList(
                    clientsResponse.Success ? clientsResponse.Data ?? new List<ClientDto>() : new List<ClientDto>(), 
                    "Id", "FullName");

                ViewData["ProfessionalId"] = new SelectList(
                    staffResponse.Success ? staffResponse.Data ?? new List<ProfessionalDto>() : new List<ProfessionalDto>(), 
                    "ProfessionalId", "Name");

                ViewData["ServiceId"] = new SelectList(
                    servicesResponse.Success ? servicesResponse.Data ?? new List<ServiceDto>() : new List<ServiceDto>(), 
                    "ServiceId", "Name");

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create appointment data");
                ViewData["CustomerId"] = new SelectList(new List<ClientDto>(), "ClientId", "Name");
                ViewData["ProfessionalId"] = new SelectList(new List<ProfessionalDto>(), "ProfessionalId", "Name");
                ViewData["ServiceId"] = new SelectList(new List<ServiceDto>(), "ServiceId", "Name");
                return View();
            }
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(AppointmentDto appointment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _appointmentsService.CreateAsync(appointment);

                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Appointment created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to create appointment: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to create appointment: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating appointment");
                    TempData["ErrorMessage"] = "An error occurred while creating the appointment.";
                }
            }

            // Recarregar dados para os dropdowns em caso de erro
            try
            {
                var clientsResponse = await _clientsService.GetAllAsync();
                var staffResponse = await _staffService.GetAllAsync();
                var servicesResponse = await _servicesService.GetAllAsync();

                ViewData["CustomerId"] = new SelectList(
                    clientsResponse.Success ? clientsResponse.Data ?? new List<ClientDto>() : new List<ClientDto>(), 
                    "ClientId", "Name", appointment.ClientId);

                ViewData["ProfessionalId"] = new SelectList(
                    staffResponse.Success ? staffResponse.Data ?? new List<ProfessionalDto>() : new List<ProfessionalDto>(), 
                    "ProfessionalId", "Specialty", appointment.ProfessionalId);

                ViewData["ServiceId"] = new SelectList(
                    servicesResponse.Success ? servicesResponse.Data ?? new List<ServiceDto>() : new List<ServiceDto>(), 
                    "ServiceId", "Name", appointment.ServiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdown data for create");
                ViewData["CustomerId"] = new SelectList(new List<ClientDto>(), "ClientId", "Name");
                ViewData["ProfessionalId"] = new SelectList(new List<ProfessionalDto>(), "ProfessionalId", "Name");
                ViewData["ServiceId"] = new SelectList(new List<ServiceDto>(), "ServiceId", "Name");
            }

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

            try
            {
                var response = await _appointmentsService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    // Buscar dados para os dropdowns
                    var clientsResponse = await _clientsService.GetAllAsync();
                    var staffResponse = await _staffService.GetAllAsync();
                    var servicesResponse = await _servicesService.GetAllAsync();

                    ViewData["CustomerId"] = new SelectList(
                        clientsResponse.Success ? clientsResponse.Data ?? new List<ClientDto>() : new List<ClientDto>(), 
                        "ClientId", "Name", response.Data.ClientId);

                    ViewData["ProfessionalId"] = new SelectList(
                        staffResponse.Success ? staffResponse.Data ?? new List<ProfessionalDto>() : new List<ProfessionalDto>(), 
                        "ProfessionalId", "Name", response.Data.ProfessionalId);

                    ViewData["ServiceId"] = new SelectList(
                        servicesResponse.Success ? servicesResponse.Data ?? new List<ServiceDto>() : new List<ServiceDto>(), 
                        "ServiceId", "Name", response.Data.ServiceId);

                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch appointment for edit {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment for edit {Id}", id);
                return NotFound();
            }
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, AppointmentDto appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _appointmentsService.UpdateAsync(id, appointment);

                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Appointment updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update appointment: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to update appointment: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating appointment");
                    TempData["ErrorMessage"] = "An error occurred while updating the appointment.";
                }
            }

            // Recarregar dados para os dropdowns em caso de erro
            try
            {
                var clientsResponse = await _clientsService.GetAllAsync();
                var staffResponse = await _staffService.GetAllAsync();
                var servicesResponse = await _servicesService.GetAllAsync();

                ViewData["CustomerId"] = new SelectList(
                    clientsResponse.Success ? clientsResponse.Data ?? new List<ClientDto>() : new List<ClientDto>(), 
                    "ClientId", "Name", appointment.ClientId);

                ViewData["ProfessionalId"] = new SelectList(
                    staffResponse.Success ? staffResponse.Data ?? new List<ProfessionalDto>() : new List<ProfessionalDto>(), 
                    "ProfessionalId", "Specialty", appointment.ProfessionalId);

                ViewData["ServiceId"] = new SelectList(
                    servicesResponse.Success ? servicesResponse.Data ?? new List<ServiceDto>() : new List<ServiceDto>(), 
                    "ServiceId", "Name", appointment.ServiceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dropdown data for edit");
                ViewData["CustomerId"] = new SelectList(new List<ClientDto>(), "ClientId", "Name");
                ViewData["ProfessionalId"] = new SelectList(new List<ProfessionalDto>(), "ProfessionalId", "Name");
                ViewData["ServiceId"] = new SelectList(new List<ServiceDto>(), "ServiceId", "Name");
            }

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

            try
            {
                var response = await _appointmentsService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch appointment for delete {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching appointment for delete {Id}", id);
                return NotFound();
            }
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _appointmentsService.DeleteAsync(id);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Appointment deleted successfully!";
                }
                else
                {
                    _logger.LogError("Failed to delete appointment: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Failed to delete appointment: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment");
                TempData["ErrorMessage"] = "An error occurred while deleting the appointment.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int professionalId, DateTime date)
        {
            try
            {
                Console.WriteLine($"=== BUSCANDO HOR�RIOS DISPON�VEIS ===");
                Console.WriteLine($"ProfissionalId: {professionalId}, Data: {date:dd/MM/yyyy}");

                // Implementar endpoint na API para buscar hor�rios dispon�veis quando dispon�vel
                return Json(new { success = true, availableTimes = new List<string>(), message = "Feature not yet implemented in API" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERRO ao buscar hor�rios: {ex.Message}");
                return Json(new { success = false, message = "Erro ao buscar hor�rios dispon�veis" });
            }
        }

        // POST: Admin/Appointments/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            try
            {
                // Implementar endpoint na API para atualizar status quando dispon�vel
                return Json(new { success = true, message = "Status update feature not yet implemented in API" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating appointment status");
                return Json(new { success = false, message = "Error updating status" });
            }
        }

    }
}
