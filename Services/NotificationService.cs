using Microsoft.AspNetCore.SignalR;
using Sistema.Data.Entities;

namespace Sistema.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string message, string type = "info", string userId = null);
        Task SendCashRegisterNotificationAsync(string message, decimal? amount = null);
        Task SendAppointmentNotificationAsync(string message, int appointmentId);
        Task SendFinancialNotificationAsync(string message, decimal amount);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationAsync(string message, string type = "info", string userId = null)
        {
            try
            {
                var notification = new
                {
                    Message = message,
                    Type = type,
                    Timestamp = DateTime.Now,
                    UserId = userId
                };

                if (string.IsNullOrEmpty(userId))
                {
                    // Enviar para todos os usuários conectados
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                }
                else
                {
                    // Enviar para usuário específico
                    await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
                }

                _logger.LogInformation($"Notificação enviada: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação");
            }
        }

        public async Task SendCashRegisterNotificationAsync(string message, decimal? amount = null)
        {
            var notificationMessage = amount.HasValue 
                ? $"{message} - Valor: € {amount.Value:N2}"
                : message;

            await SendNotificationAsync(notificationMessage, "cash", null);
        }

        public async Task SendAppointmentNotificationAsync(string message, int appointmentId)
        {
            var notificationMessage = $"{message} - Agendamento ID: {appointmentId}";
            await SendNotificationAsync(notificationMessage, "appointment", null);
        }

        public async Task SendFinancialNotificationAsync(string message, decimal amount)
        {
            var notificationMessage = $"{message} - Valor: € {amount:N2}";
            await SendNotificationAsync(notificationMessage, "financial", null);
        }
    }

    // Hub para notificações em tempo real
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
