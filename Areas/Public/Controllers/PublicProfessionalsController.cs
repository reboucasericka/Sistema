using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicProfessionalsController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IProfessionalRepository _professionalRepository;

        public PublicProfessionalsController(SistemaDbContext context, IProfessionalRepository professionalRepository)
        {
            _context = context;
            _professionalRepository = professionalRepository;
        }

        // GET: Public/Professionals
        public async Task<IActionResult> Index()
        {
            var professionals = _professionalRepository.GetActiveProfessionals().OrderBy(p => p.Name);
            return View(await professionals.ToListAsync());
        }

        // GET: Public/Professionals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _professionalRepository.GetByIdWithIncludesAsync(id.Value);

            if (professional == null || !professional.IsActive)
            {
                return NotFound();
            }

            return View(professional);
        }
    }
}
