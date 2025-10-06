using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Admin;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceProfessionalsController : Controller
    {
        private readonly SistemaDbContext _context;

        public ServiceProfessionalsController(SistemaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Edit(int serviceId)
        {
            var service = await _context.Service
                .Include(s => s.ProfessionalServices)
                .ThenInclude(ps => ps.Professional)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null) return NotFound();

            var allProfessionals = await _context.Professionals
                .Where(p => p.IsActive)
                .ToListAsync();

            var viewModel = new ServiceProfessionalViewModel
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                SelectedProfessionals = service.ProfessionalServices?.Select(ps => ps.ProfessionalId).ToList(),
                AvailableProfessionals = allProfessionals.Select(p => new SelectListItem
                {
                    Value = p.ProfessionalId.ToString(),
                    Text = p.Name
                })
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceProfessionalViewModel model)
        {
            var service = await _context.Service
                .Include(s => s.ProfessionalServices)
                .FirstOrDefaultAsync(s => s.Id == model.ServiceId);

            if (service == null) return NotFound();

            // Remove existing links
            if (service.ProfessionalServices?.Any() == true)
            {
                _context.ProfessionalServices.RemoveRange(service.ProfessionalServices);
            }

            // Add new links
            if (model.SelectedProfessionals != null)
            {
                foreach (var professionalId in model.SelectedProfessionals)
                {
                    _context.ProfessionalServices.Add(new ProfessionalService
                    {
                        ServiceId = model.ServiceId,
                        ProfessionalId = professionalId
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Profissionais vinculados com sucesso!";
            return RedirectToAction("Index", "Services", new { area = "Admin" });
        }
    }
}
