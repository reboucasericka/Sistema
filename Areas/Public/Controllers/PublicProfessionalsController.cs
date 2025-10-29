using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicProfessionalsController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicProfessionalsController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Public/Professionals
        public async Task<IActionResult> Index()
        {
            // Limpar o ChangeTracker para forçar atualização
            _context.ChangeTracker.Clear();

            var professionals = await _context.Professionals
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            return View(professionals);
        }

        // GET: Public/Professionals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Limpar o ChangeTracker para forçar atualização
            _context.ChangeTracker.Clear();

            var professional = await _context.Professionals
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .FirstOrDefaultAsync(p => p.ProfessionalId == id.Value);

            if (professional == null || !professional.IsActive)
            {
                return NotFound();
            }

            return View(professional);
        }
    }
}