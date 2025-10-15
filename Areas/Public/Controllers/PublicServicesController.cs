using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicServicesController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicServicesController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Public/Services
        public async Task<IActionResult> Index()
        {
            var services = await _context.Service
                .Include(s => s.Category)
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(services);
        }

        // GET: Public/Services/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.Service
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (service == null)
                return NotFound();

            return View(service);
        }
    }
}
