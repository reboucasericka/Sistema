using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using System.Security.Claims;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    [Authorize(Roles = "Customer")]
    public class PublicFeedbackController : Controller
    {
        private readonly SistemaDbContext _context;

        public PublicFeedbackController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Public/PublicFeedback/Add
        [HttpGet]
        public async Task<IActionResult> Add(int appointmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var clientId = int.Parse(userId);

            // Verificar se o agendamento pertence ao cliente e está concluído
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Professional)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && 
                                         a.CustomerId == clientId && 
                                         (a.Status != null && (a.Status.ToLower() == "concluído" || a.Status.ToLower() == "completed")));

            if (appointment == null)
            {
                TempData["Error"] = "Agendamento não encontrado ou não está disponível para avaliação.";
                return RedirectToAction("MyAppointments", "PublicClientPanel");
            }

            // Verificar se já existe feedback para este agendamento
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.AppointmentId == appointmentId);

            if (existingFeedback != null)
            {
                TempData["Info"] = "Você já avaliou este agendamento.";
                return RedirectToAction("MyAppointments", "PublicClientPanel");
            }

            ViewBag.Appointment = appointment;
            return View();
        }

        // POST: Public/PublicFeedback/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Feedback model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var clientId = int.Parse(userId);

            // Verificar se o agendamento pertence ao cliente
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId && 
                                         a.CustomerId == clientId &&
                                         (a.Status != null && (a.Status.ToLower() == "concluído" || a.Status.ToLower() == "completed")));

            if (appointment == null)
            {
                TempData["Error"] = "Agendamento não encontrado ou não está disponível para avaliação.";
                return RedirectToAction("MyAppointments", "PublicClientPanel");
            }

            // Verificar se já existe feedback
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.AppointmentId == model.AppointmentId);

            if (existingFeedback != null)
            {
                TempData["Info"] = "Você já avaliou este agendamento.";
                return RedirectToAction("MyAppointments", "PublicClientPanel");
            }

            if (ModelState.IsValid)
            {
                model.ClientId = clientId;
                model.Date = DateTime.Now;

                _context.Feedbacks.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Obrigado pelo seu feedback! Sua avaliação foi registrada com sucesso.";
                return RedirectToAction("MyAppointments", "PublicClientPanel");
            }

            // Recarregar dados do agendamento para a view
            appointment = await _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Professional)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);

            ViewBag.Appointment = appointment;
            return View(model);
        }

        // GET: Public/PublicFeedback/List
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

            var clientId = int.Parse(userId);

            var feedbacks = await _context.Feedbacks
                .Include(f => f.Appointment)
                    .ThenInclude(a => a.Service)
                .Include(f => f.Appointment)
                    .ThenInclude(a => a.Professional)
                .Where(f => f.ClientId == clientId)
                .OrderByDescending(f => f.Date)
                .ToListAsync();

            return View(feedbacks);
        }

        // GET: Public/PublicFeedback/GetFeedbackStats
        [HttpGet]
        public async Task<IActionResult> GetFeedbackStats()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { error = "Usuário não autenticado" });
            }

            var clientId = int.Parse(userId);

            var feedbacks = await _context.Feedbacks
                .Where(f => f.ClientId == clientId)
                .ToListAsync();

            if (!feedbacks.Any())
            {
                return Json(new { averageRating = 0, totalFeedbacks = 0 });
            }

            var averageRating = feedbacks.Average(f => f.Rating);
            var totalFeedbacks = feedbacks.Count;

            return Json(new { 
                averageRating = Math.Round(averageRating, 1), 
                totalFeedbacks 
            });
        }
    }
}
