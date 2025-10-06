using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Models;
using Sistema.Helpers;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProfessionalController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;
        private readonly IImageHelper _imageHelper;

        public ProfessionalController(SistemaDbContext context, IConverterHelper converterHelper, IUserHelper userHelper, IImageHelper imageHelper)
        {
            _context = context;
            _converterHelper = converterHelper;
            _userHelper = userHelper;
            _imageHelper = imageHelper;
        }

        // GET: Admin/Professional
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SpecialtySortParm"] = sortOrder == "specialty" ? "specialty_desc" : "specialty";
            ViewData["StatusSortParm"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["CurrentFilter"] = searchString;

            var professionals = from p in _context.Professionals
                               .Include(p => p.User)
                               .Include(p => p.ProfessionalServices)
                               .Include(p => p.Appointments)
                               select p;

            if (!string.IsNullOrEmpty(searchString))
            {
                professionals = professionals.Where(p => p.Name.Contains(searchString) 
                                                     || p.Specialty.Contains(searchString)
                                                     || p.User.Email.Contains(searchString));
            }

            professionals = sortOrder switch
            {
                "name_desc" => professionals.OrderByDescending(p => p.Name),
                "specialty" => professionals.OrderBy(p => p.Specialty),
                "specialty_desc" => professionals.OrderByDescending(p => p.Specialty),
                "status" => professionals.OrderBy(p => p.IsActive),
                "status_desc" => professionals.OrderByDescending(p => p.IsActive),
                _ => professionals.OrderBy(p => p.Name)
            };

            int pageSize = 10;
            var paginatedProfessionals = await PaginatedList<Professional>.CreateAsync(professionals.AsNoTracking(), pageNumber ?? 1, pageSize);
            
            var professionalViewModels = paginatedProfessionals.Select(p => _converterHelper.ToProfessionalViewModel(p)).ToList();
            
            return View(professionalViewModels);
        }

        // GET: Admin/Professional/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _context.Professionals
                .Include(p => p.User)
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Customer)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(p => p.Schedules)
                .FirstOrDefaultAsync(m => m.ProfessionalId == id);

            if (professional == null)
            {
                return NotFound();
            }

            var professionalViewModel = _converterHelper.ToProfessionalViewModel(professional);
            return View(professionalViewModel);
        }

        // GET: Admin/Professional/Create
        public async Task<IActionResult> Create()
        {
            // Get users who don't have a professional profile yet
            var availableUsers = await _context.Users
                .Where(u => !_context.Professionals.Any(p => p.UserId == u.Id))
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync();

            ViewBag.AvailableUsers = availableUsers;
            return View();
        }

        // POST: Admin/Professional/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfessionalCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if user already has a professional profile
                if (await _context.Professionals.AnyAsync(p => p.UserId == model.UserId))
                {
                    ModelState.AddModelError("UserId", "This user already has a professional profile.");
                    var availableUsers = await _context.Users
                        .Where(u => !_context.Professionals.Any(p => p.UserId == u.Id))
                        .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                        .ToListAsync();
                    ViewBag.AvailableUsers = availableUsers;
                    return View(model);
                }

                // Process photo upload
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("PhotoFile", "Apenas arquivos JPG, PNG, GIF e WebP são permitidos.");
                        var availableUsersForValidation = await _context.Users
                            .Where(u => !_context.Professionals.Any(p => p.UserId == u.Id))
                            .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                            .ToListAsync();
                        ViewBag.AvailableUsers = availableUsersForValidation;
                        return View(model);
                    }

                    // Validate file size (2MB max)
                    if (model.PhotoFile.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("PhotoFile", "O tamanho do arquivo não pode exceder 2MB.");
                        var availableUsersForValidation = await _context.Users
                            .Where(u => !_context.Professionals.Any(p => p.UserId == u.Id))
                            .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                            .ToListAsync();
                        ViewBag.AvailableUsers = availableUsersForValidation;
                        return View(model);
                    }

                    model.PhotoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "professionals");
                }

                var professional = _converterHelper.ToProfessional(model);
                _context.Add(professional);
                await _context.SaveChangesAsync();

                await LogProfessionalAction("Create", professional.ProfessionalId, professional.Name);
                TempData["SuccessMessage"] = "Profissional criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            var availableUsersForView = await _context.Users
                .Where(u => !_context.Professionals.Any(p => p.UserId == u.Id))
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync();
            ViewBag.AvailableUsers = availableUsersForView;
            return View(model);
        }

        // GET: Admin/Professional/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _context.Professionals
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ProfessionalId == id);

            if (professional == null)
            {
                return NotFound();
            }

            var model = _converterHelper.ToProfessionalEditViewModel(professional);
            return View(model);
        }

        // POST: Admin/Professional/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProfessionalEditViewModel model)
        {
            if (id != model.ProfessionalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var professional = await _context.Professionals.FindAsync(id);
                    if (professional == null)
                    {
                        return NotFound();
                    }

                    // Update basic fields
                    professional.Name = model.Name;
                    professional.Specialty = model.Specialty;
                    professional.DefaultCommission = model.DefaultCommission;
                    professional.IsActive = model.IsActive;

                    // Process photo upload
                    if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                    {
                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("PhotoFile", "Apenas arquivos JPG, PNG, GIF e WebP são permitidos.");
                            return View(model);
                        }

                        // Validate file size (2MB max)
                        if (model.PhotoFile.Length > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("PhotoFile", "O tamanho do arquivo não pode exceder 2MB.");
                            return View(model);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(professional.PhotoPath))
                        {
                            _imageHelper.DeleteImage(professional.PhotoPath, "professionals");
                        }

                        // Upload new image
                        professional.PhotoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "professionals");
                    }

                    _context.Update(professional);
                    await _context.SaveChangesAsync();

                    await LogProfessionalAction("Update", professional.ProfessionalId, professional.Name);
                    TempData["SuccessMessage"] = "Profissional atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfessionalExists(model.ProfessionalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Erro ao atualizar profissional: {ex.Message}";
                    return View(model);
                }
                return RedirectToAction(nameof(Edit), new { id });
            }
            return View(model);
        }

        // GET: Admin/Professional/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _context.Professionals
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(m => m.ProfessionalId == id);

            if (professional == null)
            {
                return NotFound();
            }

            return View(professional);
        }

        // POST: Admin/Professional/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professional = await _context.Professionals
                .Include(p => p.Appointments)
                .FirstOrDefaultAsync(p => p.ProfessionalId == id);

            if (professional != null)
            {
                // Check if professional has appointments
                if (professional.Appointments.Any())
                {
                    // Soft delete - deactivate instead of hard delete
                    professional.IsActive = false;
                    _context.Update(professional);
                    await LogProfessionalAction("SoftDelete", professional.ProfessionalId, professional.Name);
                    TempData["SuccessMessage"] = "Professional deactivated successfully (has appointments)!";
                }
                else
                {
                    // Hard delete - no appointments
                    _context.Professionals.Remove(professional);
                    await LogProfessionalAction("Delete", professional.ProfessionalId, professional.Name);
                    TempData["SuccessMessage"] = "Professional deleted successfully!";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Professional/Appointments/5
        public async Task<IActionResult> Appointments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _context.Professionals
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Customer)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(m => m.ProfessionalId == id);

            if (professional == null)
            {
                return NotFound();
            }

            var appointments = professional.Appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            ViewBag.ProfessionalName = professional.Name;
            return View(appointments);
        }

        // POST: Admin/Professional/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var professional = await _context.Professionals.FindAsync(id);
            if (professional != null)
            {
                professional.IsActive = !professional.IsActive;
                _context.Update(professional);
                await _context.SaveChangesAsync();

                await LogProfessionalAction("ToggleStatus", professional.ProfessionalId, professional.Name);
                TempData["SuccessMessage"] = $"Professional {(professional.IsActive ? "activated" : "deactivated")} successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProfessionalExists(int id)
        {
            return _context.Professionals.Any(e => e.ProfessionalId == id);
        }

        private async Task LogProfessionalAction(string action, int professionalId, string professionalName)
        {
            var user = await _userHelper.GetUserByEmailAsync(User.Identity?.Name);
            if (user != null)
            {
                var log = new AccessLog
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = (await _userHelper.GetUserRolesAsync(user)).FirstOrDefault(),
                    Action = $"Professional {action}: {professionalName} (ID: {professionalId})",
                    DateTime = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.AccessLogs.Add(log);
                await _context.SaveChangesAsync();
            }
        }
    }
}
