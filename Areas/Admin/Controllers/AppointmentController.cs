using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Admin;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AppointmentController : Controller
    {
        private readonly SistemaDbContext _context;

        public AppointmentController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Appointment
        public async Task<IActionResult> Index(string? status, int? professionalId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Appointments
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (professionalId.HasValue)
            {
                query = query.Where(a => a.ProfessionalId == professionalId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.StartTime >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.StartTime <= endDate.Value);
            }

            var appointments = await query
                .OrderByDescending(a => a.StartTime)
                .ToListAsync();

            // Get filter options
            ViewBag.Professionals = await _context.Professionals
                .Select(p => new { p.ProfessionalId, p.Name })
                .ToListAsync();

            ViewBag.Statuses = new List<string> { "Pending", "Confirmed", "Completed", "Canceled" };

            return View(appointments);
        }

        // GET: Admin/Appointment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // GET: Admin/Appointment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            ViewBag.Statuses = new List<string> { "Pending", "Confirmed", "Completed", "Canceled" };
            return View(appointment);
        }

        // POST: Admin/Appointment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,CustomerId,ProfessionalId,ServiceId,StartTime,EndTime,Status,Notes,TotalPrice,IsActive")] Appointment appointment)
        {
            if (id != appointment.AppointmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Log the action
                    var accessLog = new AccessLog
                    {
                        UserId = User.Identity.Name,
                        Action = $"Updated appointment {appointment.AppointmentId}",
                        Timestamp = DateTime.Now,
                        Details = $"Status changed to: {appointment.Status}"
                    };
                    _context.AccessLogs.Add(accessLog);

                    _context.Update(appointment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Appointment updated successfully!";
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

            ViewBag.Statuses = new List<string> { "Pending", "Confirmed", "Completed", "Canceled" };
            return View(appointment);
        }

        // GET: Admin/Appointment/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .ThenInclude(c => c.User)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(m => m.AppointmentId == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        // POST: Admin/Appointment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                // Log the action
                var accessLog = new AccessLog
                {
                    UserId = User.Identity.Name,
                    Action = $"Deleted appointment {appointment.AppointmentId}",
                    Timestamp = DateTime.Now,
                    Details = $"Customer: {appointment.CustomerId}, Service: {appointment.ServiceId}"
                };
                _context.AccessLogs.Add(accessLog);

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Appointment deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Appointment/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null)
            {
                return Json(new { success = false, message = "Appointment not found" });
            }

            var oldStatus = appointment.Status;
            appointment.Status = status;

            // Log the action
            var accessLog = new AccessLog
            {
                UserId = User.Identity.Name,
                Action = $"Updated appointment status",
                Timestamp = DateTime.Now,
                Details = $"Appointment {appointmentId}: {oldStatus} → {status}"
            };
            _context.AccessLogs.Add(accessLog);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Status updated successfully" });
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.AppointmentId == id);
        }
    }
}
