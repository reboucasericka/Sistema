using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Sistema.Helpers;
using Sistema.Models.Admin;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminServicesController : Controller
    {
        private readonly ApiServicesService _servicesService;
        private readonly IStorageHelper _storageHelper;
        private readonly ILogger<AdminServicesController> _logger;

        public AdminServicesController(ApiServicesService servicesService, IStorageHelper storageHelper, ILogger<AdminServicesController> logger)
        {
            _servicesService = servicesService;
            _storageHelper = storageHelper;
            _logger = logger;
        }

        // GET: Admin/Services
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _servicesService.GetAllAsync();
                
                if (response.Success && response.Data != null)
                {
                    var services = response.Data.OrderBy(s => s.Name).ToList();
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
                _logger.LogError(ex, "Error loading services");
                TempData["ErrorMessage"] = "An error occurred while loading services.";
                return View(new List<ServiceDto>());
            }
        }

        // GET: Admin/Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _servicesService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch service {Id}: {Message}", id, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service {Id}", id);
                return NotFound();
            }
        }

        // GET: Admin/Services/Create
        [HttpGet]
        public IActionResult Create()
        {
            // TODO: Buscar categorias via API quando o endpoint estiver disponível
            ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name");
            return View();
        }

        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminServiceCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string photoPath = string.Empty;
                    if (model.file != null)
                    {
                        photoPath = await _storageHelper.UploadAsync(model.file, "services");
                    }

                    var serviceDto = new ServiceDto
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        DurationMinutes = int.TryParse(model.Duration, out int duration) ? duration : 0,
                        Category = model.ServiceCategoryId.ToString(),
                        IsActive = model.IsActive
                    };

                    var response = await _servicesService.CreateAsync(serviceDto);

                    if (response.Success)
                    {
                        // Log access
                        await LogAccess("CREATE", $"Service created: {model.Name}");

                        TempData["SuccessMessage"] = "Service created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to create service: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to create service: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating service");
                    TempData["ErrorMessage"] = "An error occurred while creating the service.";
                }
            }

            // TODO: Buscar categorias via API quando o endpoint estiver disponível
            ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name", model.ServiceCategoryId);
            return View(model);
        }

        // GET: Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? serviceId)
        {
            if (serviceId == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _servicesService.GetByIdAsync(serviceId.Value);

                if (response.Success && response.Data != null)
                {
                    var model = new AdminServiceEditViewModel
                    {
                        ServiceId = response.Data.ServiceId,
                        Name = response.Data.Name,
                        Description = response.Data.Description,
                        Price = response.Data.Price,
                        Duration = response.Data.DurationMinutes.ToString(),
                        ServiceCategoryId = int.TryParse(response.Data.Category, out int catId) ? catId : 0,
                        IsActive = response.Data.IsActive
                    };

                    // TODO: Buscar categorias via API quando o endpoint estiver disponível
                    ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name", model.ServiceCategoryId);
                    return View(model);
                }
                else
                {
                    _logger.LogError("Failed to fetch service for edit {Id}: {Message}", serviceId, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service for edit {Id}", serviceId);
                return NotFound();
            }
        }

        // POST: Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int serviceId, AdminServiceEditViewModel model)
        {
            if (serviceId != model.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Se uma nova foto foi enviada, fazer upload
                    int? imageId = model.ImageId != null ? int.Parse(model.ImageId) : null;
                    if (model.file != null)
                    {
                        if (imageId != null)
                        {
                            await _storageHelper.DeleteAsync(imageId.ToString(), "services");
                        }
                        string photoPath = await _storageHelper.UploadAsync(model.file, "services");
                        imageId = string.IsNullOrEmpty(photoPath) ? null : int.Parse(photoPath);
                    }

                    var serviceDto = new ServiceDto
                    {
                        ServiceId = model.ServiceId,
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        DurationMinutes = int.TryParse(model.Duration, out int duration) ? duration : 0,
                        Category = model.ServiceCategoryId.ToString(),
                        IsActive = model.IsActive
                    };

                    var response = await _servicesService.UpdateAsync(serviceId, serviceDto);

                    if (response.Success)
                    {
                        // Log access
                        await LogAccess("UPDATE", $"Service updated: {model.Name}");

                        TempData["SuccessMessage"] = "Service updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update service: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to update service: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating service");
                    TempData["ErrorMessage"] = "An error occurred while updating the service.";
                }
            }

            // TODO: Buscar categorias via API quando o endpoint estiver disponível
            ViewData["ServiceCategories"] = new SelectList(new List<object>(), "CategoryId", "Name", model.ServiceCategoryId);
            return View(model);
        }

        // GET: Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? serviceId)
        {
            if (serviceId == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _servicesService.GetByIdAsync(serviceId.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch service for delete {Id}: {Message}", serviceId, response.Message);
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service for delete {Id}", serviceId);
                return NotFound();
            }
        }

        // POST: Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int serviceId)
        {
            try
            {
                // TODO: Buscar o serviço primeiro para deletar a imagem
                // var service = await _servicesService.GetByIdAsync(serviceId);
                // if (service.Success && service.Data?.ImageId != Guid.Empty)
                // {
                //     await _storageHelper.DeleteAsync(service.Data.ImageId.ToString(), "services");
                // }

                var response = await _servicesService.DeleteAsync(serviceId);

                if (response.Success)
                {
                    // Log access
                    await LogAccess("DELETE", $"Service deleted: {serviceId}");

                    TempData["SuccessMessage"] = "Service deleted successfully!";
                }
                else
                {
                    _logger.LogError("Failed to delete service: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Failed to delete service: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service");
                TempData["ErrorMessage"] = "An error occurred while deleting the service.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LogAccess(string action, string details)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userId))
                {
                    // TODO: Implementar log via API quando o endpoint estiver disponível
                    _logger.LogInformation("Access Log: {Action} - {Details} by User {UserId}", action, details, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging access");
            }
        }
    }
}