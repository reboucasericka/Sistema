using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using System.Linq;
using Microsoft.AspNetCore.Authorization;


namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AppointmentsController : Controller
    {
        private readonly SistemaDbContext _context;

        public AppointmentsController(SistemaDbContext context)
        {
            _context = context;
        }


        // ✅ Página pública de agendamento online
        [AllowAnonymous] // Não pede login
        public async Task<IActionResult> Public()
        {
            ViewData["Title"] = "Agendamento Online";

            // Carregar os dados necessários
            ViewBag.Services = await _context.Service
                .Include(s => s.Category)
                .ToListAsync();

            ViewBag.Plans = await _context.Plans.ToListAsync();

            ViewBag.Professionals = await _context.Professionals
                .Include(p => p.User) // para exibir nome e foto do usuário vinculado
                .ToListAsync();

            ViewBag.Reviews = await _context.ServiceReviews
                .Include(r => r.Client)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return View();
        }

        // GET: Appointments - Lista administrativa (apenas para admins)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var sistemaDbContext = _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .ThenInclude(p => p.User)
                .Include(a => a.Service);
            return View(await sistemaDbContext.ToListAsync());
        }

        // GET: Appointments/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name");
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty");
            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Name");
            return View();
        }

        // POST: Appointments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create( Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name", appointment.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty", appointment.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Name", appointment.ServiceId);
            return View(appointment);
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name", appointment.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty", appointment.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Name", appointment.ServiceId);
            return View(appointment);
        }

        // POST: Appointments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,  Appointment appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppointmentExists(appointment.AppointmentId))
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
            ViewData["CustomerId"] = new SelectList(_context.Customers, "CustomerId", "Name", appointment.CustomerId);
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty", appointment.ProfessionalId);
            ViewData["ServiceId"] = new SelectList(_context.Service, "Id", "Name", appointment.ServiceId);
            return View(appointment);
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAvailableTimes(int professionalId, DateTime date)
        {
            // 1. Busca todos os horários do profissional para o dia da semana
            var schedules = await _context.ProfessionalSchedules
                .Where(s => s.ProfessionalId == professionalId && s.DayOfWeek == date.DayOfWeek)
                .ToListAsync();

            if (schedules == null || !schedules.Any())
            {
                return Json(new List<string>()); // Sem disponibilidade
            }

            // 2. Gera todos os slots possíveis (30 min) com base nos horários encontrados
            var allSlots = new List<string>();
            foreach (var schedule in schedules)
            {
                for (var t = schedule.StartTime; t < schedule.EndTime; t = t.Add(TimeSpan.FromMinutes(30)))
                {
                    allSlots.Add(t.ToString(@"hh\:mm"));
                }
            }

            // 3. Busca os horários já ocupados
            var bookedTimes = await _context.Appointments
                .Where(a => a.ProfessionalId == professionalId && a.StartTime.Date == date.Date)
                .Select(a => a.StartTime.TimeOfDay)
                .ToListAsync();

            // 4. Remove os horários ocupados
            var availableTimes = allSlots
                .Where(t => !bookedTimes.Any(bt => bt.ToString(@"hh\:mm") == t))
                .ToList();

            return Json(availableTimes);
        }
    }
}
