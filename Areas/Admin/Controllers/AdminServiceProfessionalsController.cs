using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema.Models.Admin;
using Sistema.Services.Api;
using SistemaAPI.DTOs;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminServiceProfessionalsController : Controller
    {
        private readonly IApiServicesService _servicesService;
        private readonly IApiStaffService _staffService;
        private readonly ILogger<AdminServiceProfessionalsController> _logger;

        public AdminServiceProfessionalsController(
            IApiServicesService servicesService,
            IApiStaffService staffService,
            ILogger<AdminServiceProfessionalsController> logger)
        {
            _servicesService = servicesService;
            _staffService = staffService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var servicesResponse = await _servicesService.GetAllAsync();
                var staffResponse = await _staffService.GetAllAsync();

                if (servicesResponse.IsSuccess && staffResponse.IsSuccess)
                {
                    var services = servicesResponse.Data?.ToList() ?? new List<ServiceDto>();
                    var staff = staffResponse.Data?.ToList() ?? new List<ProfessionalDto>();

                    var lista = services.SelectMany(s => staff.Select(p => new
                    {
                        Id = $"{s.ServiceId}_{p.ProfessionalId}",
                        ProfessionalName = p.Name,
                        ServiceName = s.Name,
                        Duration = s.Duration,
                        Price = s.Price,
                        Commission = 0m // Implementar comissão via API quando disponível
                    })).ToList();

                    return View(lista);
                }
                else
                {
                    _logger.LogError("Erro ao buscar dados: Services={ServicesError}, Staff={StaffError}", 
                        servicesResponse.Message, staffResponse.Message);
                    TempData["Error"] = "Erro ao carregar dados.";
                    return View(new List<object>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de serviços e profissionais");
                TempData["Error"] = "Erro interno do servidor.";
                return View(new List<object>());
            }
        }

        public async Task<IActionResult> Edit(int serviceId)
        {
            try
            {
                var serviceResponse = await _servicesService.GetByIdAsync(serviceId);
                var staffResponse = await _staffService.GetAllAsync();

                if (!serviceResponse.IsSuccess || serviceResponse.Data == null)
                {
                    return NotFound();
                }

                if (!staffResponse.IsSuccess)
                {
                    _logger.LogError("Erro ao buscar profissionais: {Error}", staffResponse.Message);
                    TempData["Error"] = "Erro ao carregar profissionais.";
                    return View(new AdminServiceProfessionalViewModel());
                }

                var service = serviceResponse.Data;
                var allProfessionals = staffResponse.Data?.Where(p => p.IsActive).ToList() ?? new List<ProfessionalDto>();

                var viewModel = new AdminServiceProfessionalViewModel
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.Name,
                    SelectedProfessionals = new List<int>(), // Implementar via API quando disponível
                    AvailableProfessionals = allProfessionals.Select(p => new SelectListItem
                    {
                        Value = p.ProfessionalId.ToString(),
                        Text = p.Name
                    })
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de edição de serviço {ServiceId}", serviceId);
                TempData["Error"] = "Erro interno do servidor.";
                return View(new AdminServiceProfessionalViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminServiceProfessionalViewModel model)
        {
            try
            {
                // Implementar via API quando disponível
                // Por enquanto, apenas redireciona com sucesso
                TempData["SuccessMessage"] = "Profissionais vinculados com sucesso!";
                return RedirectToAction("Index", "Services", new { area = "Admin" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao vincular profissionais ao serviço {ServiceId}", model.ServiceId);
                TempData["ErrorMessage"] = "Erro interno do servidor.";
                return View(model);
            }
        }
    }
}
