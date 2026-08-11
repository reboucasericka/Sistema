using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;
using Sistema.Models.Admin;
using System.Security.Claims;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminServicesController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IStorageHelper _storageHelper;

        public AdminServicesController(SistemaDbContext context, IStorageHelper storageHelper)
        {
            _context = context;
            _storageHelper = storageHelper;
        }

        // GET: Admin/Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.Category)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return View(services);
        }

        // GET: Admin/Services/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Category)
                .Include(s => s.ProfessionalServices)
                    .ThenInclude(ps => ps.Professional)
                .FirstOrDefaultAsync(m => m.ServiceId == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Admin/Services/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["ServiceCategories"] = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminServiceCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string photoPath = string.Empty;
                if (model.file != null)
                {
                    photoPath = await _storageHelper.UploadAsync(model.file, "services");
                }

                var service = new Service
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Duration = model.Duration,
                    CategoryId = model.ServiceCategoryId,
                    IsActive = model.IsActive,
                    ImageId = string.IsNullOrEmpty(photoPath) ? Guid.Empty : Guid.Parse(photoPath)
                };

                _context.Add(service);
                await _context.SaveChangesAsync();

                // Log access
                await LogAccess("CREATE", $"Service created: {service.Name}");

                TempData["Message"] = "Service created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["ServiceCategories"] = new SelectList(_context.Categories, "CategoryId", "Name", model.ServiceCategoryId);
            return View(model);
        }

        // GET: Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? serviceId)
        {
            if (serviceId == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return NotFound();
            }

            var model = new AdminServiceEditViewModel
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                ServiceCategoryId = service.CategoryId,
                IsActive = service.IsActive,
                ImageId = service.ImageId.ToString()
            };

            ViewData["ServiceCategories"] = new SelectList(_context.Categories, "CategoryId", "Name", model.ServiceCategoryId);
            return View(model);
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
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service == null)
                    {
                        return NotFound();
                    }

                    // Se uma nova foto foi enviada, deletar a antiga e fazer upload da nova
                    if (model.file != null)
                    {
                        if (service.ImageId != Guid.Empty)
                        {
                            await _storageHelper.DeleteAsync(service.ImageId.ToString(), "services");
                        }
                        string photoPath = await _storageHelper.UploadAsync(model.file, "services");
                        service.ImageId = string.IsNullOrEmpty(photoPath) ? Guid.Empty : Guid.Parse(photoPath);
                    }

                    service.Name = model.Name;
                    service.Description = model.Description;
                    service.Price = model.Price;
                    service.Duration = model.Duration;
                    service.CategoryId = model.ServiceCategoryId;
                    service.IsActive = model.IsActive;

                    _context.Update(service);
                    await _context.SaveChangesAsync();

                    // Log access
                    await LogAccess("UPDATE", $"Service updated: {service.Name}");

                    TempData["Message"] = "Service updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(model.ServiceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ServiceCategories"] = new SelectList(_context.Categories, "CategoryId", "Name", model.ServiceCategoryId);
            return View(model);
        }

        // GET: Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? serviceId)
        {
            if (serviceId == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.ServiceId == serviceId);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service != null)
            {
                // Deletar a foto se existir
                if (service.ImageId != Guid.Empty)
                {
                    await _storageHelper.DeleteAsync(service.ImageId.ToString(), "services");
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                // Log access
                await LogAccess("DELETE", $"Service deleted: {service.Name}");

                TempData["Message"] = "Service deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int serviceId)
        {
            return _context.Services.Any(e => e.ServiceId == serviceId);
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