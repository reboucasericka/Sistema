using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Services;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class NotificationsController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly INotificationService _notificationService;

        public NotificationsController(SistemaDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(notifications);
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (ModelState.IsValid)
            {
                notification.CreatedAt = DateTime.Now;
                notification.IsRead = false;
                
                _context.Add(notification);
                await _context.SaveChangesAsync();

                // Enviar notificação em tempo real
                await _notificationService.SendNotificationAsync(notification.Message, notification.Type);

                TempData["SuccessMessage"] = "Notificação criada e enviada com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(notification);
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                return NotFound();
            }
            return View(notification);
        }

        // POST: Notifications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Notification notification)
        {
            if (id != notification.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.Id))
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
            return View(notification);
        }

        // GET: Notifications/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Notifications/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                _context.Update(notification);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true });
        }

        // POST: Notifications/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, count = unreadNotifications.Count });
        }

        // GET: Notifications/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _context.Notifications
                .Where(n => !n.IsRead)
                .CountAsync();

            return Json(new { count });
        }

        // GET: Notifications/GetRecent
        [HttpGet]
        public async Task<IActionResult> GetRecent(int count = 10)
        {
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.Type,
                    n.CreatedAt,
                    n.IsRead,
                    TimeAgo = GetTimeAgo(n.CreatedAt)
                })
                .ToListAsync();

            return Json(notifications);
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.Id == id);
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Agora mesmo";
            else if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min atrás";
            else if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours}h atrás";
            else
                return $"{(int)timeSpan.TotalDays} dias atrás";
        }
    }
}

