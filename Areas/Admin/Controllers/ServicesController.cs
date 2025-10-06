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
    public class ServicesController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IImageHelper _imageHelper;

        public ServicesController(SistemaDbContext context, IImageHelper imageHelper)
        {
            _context = context;
            _imageHelper = imageHelper;
        }

        // GET: Admin/Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Service
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

            var service = await _context.Service
                .Include(s => s.Category)
                .Include(s => s.ProfessionalServices)
                    .ThenInclude(ps => ps.Professional)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Admin/Services/Create
        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Admin/Services/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string photoPath = string.Empty;
                if (model.PhotoFile != null)
                {
                    photoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "services");
                }

                var service = new Service
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Duration = model.Duration,
                    CategoryId = model.CategoryId,
                    IsActive = model.IsActive,
                    PhotoPath = photoPath
                };

                _context.Add(service);
                await _context.SaveChangesAsync();

                // Log access
                await LogAccess("CREATE", $"Service created: {service.Name}");

                TempData["Message"] = "Service created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Service.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            var model = new ServiceEditViewModel
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration,
                CategoryId = service.CategoryId,
                IsActive = service.IsActive,
                PhotoPath = service.PhotoPath
            };

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // POST: Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceEditViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var service = await _context.Service.FindAsync(id);
                    if (service == null)
                    {
                        return NotFound();
                    }

                    // Se uma nova foto foi enviada, deletar a antiga e fazer upload da nova
                    if (model.PhotoFile != null)
                    {
                        if (!string.IsNullOrEmpty(service.PhotoPath))
                        {
                            _imageHelper.DeleteImage(service.PhotoPath, "services");
                        }
                        service.PhotoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "services");
                    }

                    service.Name = model.Name;
                    service.Description = model.Description;
                    service.Price = model.Price;
                    service.Duration = model.Duration;
                    service.CategoryId = model.CategoryId;
                    service.IsActive = model.IsActive;

                    _context.Update(service);
                    await _context.SaveChangesAsync();

                    // Log access
                    await LogAccess("UPDATE", $"Service updated: {service.Name}");

                    TempData["Message"] = "Service updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(model.Id))
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

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Service
                .Include(s => s.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Service.FindAsync(id);
            if (service != null)
            {
                // Deletar a foto se existir
                if (!string.IsNullOrEmpty(service.PhotoPath))
                {
                    _imageHelper.DeleteImage(service.PhotoPath, "services");
                }

                _context.Service.Remove(service);
                await _context.SaveChangesAsync();

                // Log access
                await LogAccess("DELETE", $"Service deleted: {service.Name}");

                TempData["Message"] = "Service deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Service.Any(e => e.Id == id);
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