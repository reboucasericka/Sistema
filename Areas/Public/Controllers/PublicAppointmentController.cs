using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicAppointmentController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicAppointmentController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Appointment
        // GET: Appointment
        public async Task<IActionResult> Index()
        {
            var services = await _context.Service
                                         .Include(s => s.Category)
                                         .ToListAsync();

            var plans = await _context.Plans.ToListAsync(); // substitui packages
            var professionals = await _context.Professionals.ToListAsync();
            var reviews = await _context.ServiceReviews
                                        .Include(r => r.Client)
                                        .ToListAsync();

            ViewBag.Services = services;
            ViewBag.Plans = plans;
            ViewBag.Professionals = professionals;
            ViewBag.Reviews = reviews;

            return View();
        }

    }
}