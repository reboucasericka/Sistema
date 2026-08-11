using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Public;
using Sistema.Helpers;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize(Roles = "Customer")]
    public class PublicProfileController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;

        public PublicProfileController(SistemaDbContext context, IConverterHelper converterHelper, IUserHelper userHelper)
        {
            _context = context;
            _converterHelper = converterHelper;
            _userHelper = userHelper;
        }

        // GET: Public/Profile
        public async Task<IActionResult> Index()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var customer = await _context.Customers
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .FirstOrDefaultAsync(c => c.Email == currentUser.Email);

            if (customer == null)
            {
                // Create customer profile if it doesn't exist
                customer = new Customer
                {
                    Name = $"{currentUser.FirstName} {currentUser.LastName}".Trim(),
                    Email = currentUser.Email,
                    Phone = currentUser.PhoneNumber,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Log the action
                await LogProfileAction("Create", customer.CustomerId, customer.Name);
            }

            var profileViewModel = _converterHelper.ToCustomerProfileViewModel(customer);
            return View(profileViewModel);
        }

        // GET: Public/Profile/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var customer = await _context.Customers
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .FirstOrDefaultAsync(c => c.Email == currentUser.Email);

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            var dashboardViewModel = new PublicCustomerDashboardViewModel
            {
                Profile = _converterHelper.ToCustomerProfileViewModel(customer),
                RecentAppointments = customer.Appointments
                    .OrderByDescending(a => a.Date)
                    .ThenByDescending(a => a.Time)
                    .Take(5)
                    .ToList(),
                UpcomingAppointments = customer.Appointments
                    .Where(a => a.Date >= DateTime.Today && a.Status != "Cancelado")
                    .OrderBy(a => a.Date)
                    .ThenBy(a => a.Time)
                    .Take(5)
                    .ToList(),
                AvailableServices = await _context.Services
                    .Include(s => s.Category)
                    .Where(s => s.IsActive)
                    .ToListAsync(),
                AvailableProfessionals = await _context.Professionals
                    .Where(p => p.IsActive)
                    .ToListAsync()
            };

            // Calculate statistics
            var appointments = customer.Appointments.ToList();
            dashboardViewModel.TotalAppointments = appointments.Count;
            dashboardViewModel.CompletedAppointments = appointments.Count(a => a.Status == "Concluído");
            dashboardViewModel.CancelledAppointments = appointments.Count(a => a.Status == "Cancelado");
            dashboardViewModel.PendingAppointments = appointments.Count(a => a.Status == "Pendente");

            return View(dashboardViewModel);
        }

        // GET: Public/Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == currentUser.Email);

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            var profileEditViewModel = _converterHelper.ToCustomerProfileEditViewModel(customer);
            return View(profileEditViewModel);
        }

        // POST: Public/Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PublicCustomerProfileEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    return RedirectToAction("Login", "Account", new { area = "" });
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == currentUser.Email);

                if (customer == null)
                {
                    return RedirectToAction("Index");
                }

                // Check if email already exists for another customer
                if (!string.IsNullOrEmpty(model.Email) && 
                    await _context.Customers.AnyAsync(c => c.Email == model.Email && c.CustomerId != customer.CustomerId))
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                    return View(model);
                }

                // Update customer data
                customer.Name = model.Name;
                customer.Email = model.Email;
                customer.Phone = model.Phone;
                customer.Address = model.Address;
                customer.BirthDate = model.BirthDate;
                customer.Notes = model.Notes;
                customer.AllergyHistory = model.AllergyHistory;

                _context.Update(customer);
                await _context.SaveChangesAsync();

                // Log the action
                await LogProfileAction("Edit", customer.CustomerId, customer.Name);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Public/Profile/Appointments
        public async Task<IActionResult> Appointments(string status = "all")
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var customer = await _context.Customers
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .FirstOrDefaultAsync(c => c.Email == currentUser.Email);

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            var appointments = customer.Appointments
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                appointments = appointments.Where(a => a.Status == status);
            }

            ViewBag.Status = status;
            ViewBag.CustomerName = customer.Name;
            return View(await appointments.ToListAsync());
        }

        // POST: Public/Profile/CancelAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found." });
            }

            // Verify the appointment belongs to the current user
            if (appointment.Client.Email != currentUser.Email)
            {
                return Json(new { success = false, message = "You don't have permission to cancel this appointment." });
            }

            // Check if appointment can be cancelled (not in the past)
            if (appointment.Date < DateTime.Today)
            {
                return Json(new { success = false, message = "Cannot cancel past appointments." });
            }

            appointment.Status = "Cancelado";
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            // Log the action
            await LogProfileAction("CancelAppointment", appointment.AppointmentId, $"Appointment {appointment.AppointmentId}");

            return Json(new { success = true, message = "Appointment cancelled successfully!" });
        }

        // POST: Public/Profile/ConfirmAppointment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var appointment = await _context.Appointments
                .Include(a => a.Client)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found." });
            }

            // Verify the appointment belongs to the current user
            if (appointment.Client.Email != currentUser.Email)
            {
                return Json(new { success = false, message = "You don't have permission to confirm this appointment." });
            }

            appointment.Status = "Confirmado";
            appointment.ReminderSent = true;
            _context.Update(appointment);
            await _context.SaveChangesAsync();

            // Log the action
            await LogProfileAction("ConfirmAppointment", appointment.AppointmentId, $"Appointment {appointment.AppointmentId}");

            return Json(new { success = true, message = "Appointment confirmed successfully!" });
        }

        // GET: Public/Profile/History
        public async Task<IActionResult> History()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var customer = await _context.Customers
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .FirstOrDefaultAsync(c => c.Email == currentUser.Email);

            if (customer == null)
            {
                return RedirectToAction("Index");
            }

            var pastAppointments = customer.Appointments
                .Where(a => a.Date < DateTime.Today)
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToList();

            ViewBag.CustomerName = customer.Name;
            return View(pastAppointments);
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return null;

            var identityName = User.Identity.Name;
            if (string.IsNullOrEmpty(identityName))
                return null;

            // Try email first, then username
            var user = await _userHelper.GetUserByEmailAsync(identityName);
            if (user == null)
            {
                user = await _userHelper.GetUserByUsernameAsync(identityName);
            }

            return user;
        }

        private async Task LogProfileAction(string action, int entityId, string entityName)
        {
            try
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser != null)
                {
                    var accessLog = new AccessLog
                    {
                        UserId = currentUser.Id,
                        Action = $"Profile_{action}",
                        Role = "Customer",
                        Email = currentUser.Email,
                        DateTime = DateTime.Now,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                    };

                    _context.AccessLogs.Add(accessLog);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the main flow
                Console.WriteLine($"Error logging profile action: {ex.Message}");
            }
        }
    }
}
