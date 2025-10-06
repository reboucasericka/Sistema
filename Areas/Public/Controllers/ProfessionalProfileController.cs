using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Public;
using Sistema.Models;
using Sistema.Helpers;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize(Roles = "Professional")]
    public class ProfessionalProfileController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;

        public ProfessionalProfileController(SistemaDbContext context, IUserHelper userHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
        }

        // GET: Public/ProfessionalProfile/Index
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var professional = await _context.Professionals
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Client)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(p => p.Schedules)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (professional == null)
            {
                // If professional doesn't exist, create a new one
                professional = new Professional
                {
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    UserId = user.Id,
                    Specialty = "General",
                    DefaultCommission = 0,
                    IsActive = true
                };
                _context.Professionals.Add(professional);
                await _context.SaveChangesAsync();

                // Log the action
                await LogProfileAction("Create", professional.ProfessionalId, professional.Name);
            }

            var profileViewModel = _converterHelper.ToProfessionalProfileViewModel(professional);
            return View(profileViewModel);
        }

        // GET: Public/ProfessionalProfile/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var professional = await _context.Professionals
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Client)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(p => p.Schedules)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (professional == null)
            {
                return RedirectToAction("Index");
            }

            var dashboardViewModel = new ProfessionalDashboardViewModel
            {
                Profile = _converterHelper.ToProfessionalProfileViewModel(professional),
                TodayAppointments = professional.Appointments
                    .Where(a => a.StartTime.Date == DateTime.Today)
                    .OrderBy(a => a.Time)
                    .ToList(),
                UpcomingAppointments = professional.Appointments
                    .Where(a => a.Date > DateTime.Today && a.Status != "Cancelado")
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Time)
                    .Take(10)
                    .ToList(),
                RecentAppointments = professional.Appointments
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.Time)
                    .Take(5)
                    .ToList(),
                AvailableServices = await _context.Service
                    .Include(s => s.Category)
                    .Where(s => s.IsActive)
                    .ToListAsync(),
                WeeklySchedule = professional.Schedules
                    .Where(s => s.DayOfWeek >= (int)DateTime.Today.DayOfWeek)
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.Time)
                    .ToList()
            };

            // Calculate statistics
            var appointments = professional.Appointments.ToList();
            dashboardViewModel.TotalAppointments = appointments.Count;
            dashboardViewModel.CompletedAppointments = appointments.Count(a => a.Status == "Concluído");
            dashboardViewModel.CancelledAppointments = appointments.Count(a => a.Status == "Cancelado");
            dashboardViewModel.PendingAppointments = appointments.Count(a => a.Status == "Pendente");

            // Calculate earnings (simplified - based on completed appointments)
            var completedAppointments = appointments.Where(a => a.Status == "Concluído").ToList();
            dashboardViewModel.TotalEarnings = completedAppointments.Sum(a => a.Service.Price * (professional.DefaultCommission / 100));
            dashboardViewModel.MonthlyEarnings = completedAppointments
                .Where(a => a.StartTime.Month == DateTime.Now.Month && a.StartTime.Year == DateTime.Now.Year)
                .Sum(a => a.Service.Price * (professional.DefaultCommission / 100));

            return View(dashboardViewModel);
        }

        // GET: Public/ProfessionalProfile/Edit
        public async Task<IActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var professional = await _context.Professionals
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (professional == null)
            {
                return RedirectToAction("Index");
            }

            var profileEditViewModel = _converterHelper.ToProfessionalProfileEditViewModel(professional);
            return View(profileEditViewModel);
        }

        // POST: Public/ProfessionalProfile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfessionalProfileEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var professional = await _context.Professionals
                    .FirstOrDefaultAsync(p => p.ProfessionalId == model.ProfessionalId && p.UserId == currentUser.Id);

                if (professional == null)
                {
                    return NotFound();
                }

                // Update professional properties from model
                professional.Name = model.Name;
                professional.PhotoPath = model.PhotoPath;
                professional.Specialty = model.Specialty;
                professional.DefaultCommission = model.DefaultCommission;

                _context.Update(professional);
                await _context.SaveChangesAsync();

                await LogProfileAction("Update", professional.ProfessionalId, professional.Name);
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Public/ProfessionalProfile/Schedule
        public async Task<IActionResult> Schedule()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var professional = await _context.Professionals
                .Include(p => p.Schedules)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Client)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (professional == null)
            {
                return RedirectToAction("Index");
            }

            // Get appointments for the next 7 days
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(7);

            var upcomingAppointments = professional.Appointments
                .Where(a => a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Time)
                .ToList();

            ViewBag.ProfessionalName = professional.Name;
            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            return View(upcomingAppointments);
        }

        // GET: Public/ProfessionalProfile/History
        public async Task<IActionResult> History(int? pageNumber)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var professional = await _context.Professionals
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Client)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (professional == null)
            {
                return RedirectToAction("Index");
            }

            var appointments = professional.Appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .AsQueryable();

            int pageSize = 10;
            var paginatedAppointments = await PaginatedList<Appointment>.CreateAsync(appointments.AsNoTracking(), pageNumber ?? 1, pageSize);

            ViewBag.ProfessionalName = professional.Name;
            return View(paginatedAppointments);
        }

        // POST: Public/ProfessionalProfile/ConfirmAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Professional)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.Professional.UserId == user.Id);

            if (appointment != null)
            {
                appointment.Status = "Confirmado";
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                await LogProfileAction("ConfirmAppointment", appointment.AppointmentId, $"Appointment for {appointment.Customer?.Name}");
                TempData["SuccessMessage"] = "Appointment confirmed successfully!";
            }

            return RedirectToAction("Schedule");
        }

        // POST: Public/ProfessionalProfile/CancelAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Professional)
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.Professional.UserId == user.Id);

            if (appointment != null)
            {
                appointment.Status = "Cancelado";
                _context.Update(appointment);
                await _context.SaveChangesAsync();

                await LogProfileAction("CancelAppointment", appointment.AppointmentId, $"Appointment for {appointment.Customer?.Name}");
                TempData["SuccessMessage"] = "Appointment cancelled successfully!";
            }

            return RedirectToAction("Schedule");
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            return await _userHelper.GetUserByEmailAsync(User.Identity?.Name)
                ?? await _userHelper.GetUserByUsernameAsync(User.Identity?.Name);
        }

        private async Task LogProfileAction(string action, int professionalId, string professionalName)
        {
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var log = new AccessLog
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = (await _userHelper.GetUserRolesAsync(user)).FirstOrDefault(),
                    Action = $"Professional Profile {action}: {professionalName} (ID: {professionalId})",
                    DateTime = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                _context.AccessLogs.Add(log);
                await _context.SaveChangesAsync();
            }
        }
    }
}
