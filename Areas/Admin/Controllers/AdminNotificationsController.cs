using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Services.Api;
using SistemaAPI.DTOs;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminNotificationsController : Controller
    {
        private readonly IApiNotificationsService _notificationsService;
        private readonly ILogger<AdminNotificationsController> _logger;

        public AdminNotificationsController(IApiNotificationsService notificationsService, ILogger<AdminNotificationsController> logger)
        {
            _notificationsService = notificationsService;
            _logger = logger;
        }

        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _notificationsService.GetAllAsync();
                if (response.IsSuccess)
                {
                    var notifications = response.Data?.OrderByDescending(n => n.CreatedAt).Take(50).ToList() ?? new List<NotificationDto>();
                    return View(notifications);
                }
                else
                {
                    _logger.LogError("Erro ao buscar notificações: {Error}", response.Message);
                    TempData["Error"] = "Erro ao carregar notificações.";
                    return View(new List<NotificationDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar notificações");
                TempData["Error"] = "Erro interno do servidor.";
                return View(new List<NotificationDto>());
            }
        }

        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var response = await _notificationsService.GetByIdAsync(id.Value);
                if (response.IsSuccess && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar notificação {Id}", id);
                return NotFound();
            }
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notifications/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotificationDto notification)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    notification.CreatedAt = DateTime.Now;
                    notification.IsRead = false;
                    
                    var response = await _notificationsService.CreateAsync(notification);
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Notificação criada e enviada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao criar notificação.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao criar notificação");
                    TempData["ErrorMessage"] = "Erro interno do servidor.";
                }
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

            try
            {
                var response = await _notificationsService.GetByIdAsync(id.Value);
                if (response.IsSuccess && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar notificação para edição {Id}", id);
                return NotFound();
            }
        }

        // POST: Notifications/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NotificationDto notification)
        {
            if (id != notification.NotificationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _notificationsService.UpdateAsync(id, notification);
                    if (response.IsSuccess)
                    {
                        TempData["SuccessMessage"] = "Notificação atualizada com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Erro ao atualizar notificação.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao atualizar notificação {Id}", id);
                    TempData["ErrorMessage"] = "Erro interno do servidor.";
                }
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

            try
            {
                var response = await _notificationsService.GetByIdAsync(id.Value);
                if (response.IsSuccess && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar notificação para exclusão {Id}", id);
                return NotFound();
            }
        }

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _notificationsService.DeleteAsync(id);
                if (response.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Notificação excluída com sucesso!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erro ao excluir notificação.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir notificação {Id}", id);
                TempData["ErrorMessage"] = "Erro interno do servidor.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Notifications/MarkAsRead/5
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var response = await _notificationsService.MarkAsReadAsync(id);
                if (response.IsSuccess)
                {
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Erro ao marcar notificação como lida." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar notificação como lida {Id}", id);
                return Json(new { success = false, message = "Erro interno do servidor." });
            }
        }

        // POST: Notifications/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                // Implementar via API quando disponível
                // Por enquanto, retorna sucesso sem fazer nada
                return Json(new { success = true, count = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar todas as notificações como lidas");
                return Json(new { success = false, message = "Erro interno do servidor." });
            }
        }

        // GET: Notifications/GetUnreadCount
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var response = await _notificationsService.GetAllAsync();
                if (response.IsSuccess)
                {
                    var count = response.Data?.Count(n => !n.IsRead) ?? 0;
                    return Json(new { count });
                }
                else
                {
                    return Json(new { count = 0 });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter contagem de notificações não lidas");
                return Json(new { count = 0 });
            }
        }

        // GET: Notifications/GetRecent
        [HttpGet]
        public async Task<IActionResult> GetRecent(int count = 10)
        {
            try
            {
                var response = await _notificationsService.GetAllAsync();
                if (response.IsSuccess)
                {
                    var notifications = response.Data?
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(count)
                        .Select(n => new
                        {
                            n.NotificationId,
                            n.Message,
                            n.Type,
                            n.CreatedAt,
                            n.IsRead,
                            TimeAgo = GetTimeAgo(n.CreatedAt)
                        })
                        .ToList();

                    return Json(notifications);
                }
                else
                {
                    return Json(new List<object>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter notificações recentes");
                return Json(new List<object>());
            }
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


