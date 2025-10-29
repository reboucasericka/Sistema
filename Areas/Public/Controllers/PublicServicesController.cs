using Microsoft.AspNetCore.Mvc;
using Sistema.Services.Api;
using SistemaAPI.DTOs;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicServicesController : Controller
    {
        private readonly IApiServicesService _servicesService;
        private readonly ILogger<PublicServicesController> _logger;

        public PublicServicesController(IApiServicesService servicesService, ILogger<PublicServicesController> logger)
        {
            _servicesService = servicesService;
            _logger = logger;
        }

        // GET: Public/Services
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _servicesService.GetAllAsync();
                
                if (response.IsSuccess && response.Data != null)
                {
                    var services = response.Data.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
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

        // GET: Public/Services/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _servicesService.GetByIdAsync(id);
                
                if (!response.IsSuccess || response.Data == null)
                {
                    return NotFound();
                }

                return View(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service details");
                TempData["ErrorMessage"] = "An error occurred while loading service details.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
