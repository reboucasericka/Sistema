using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sistema.Data;
using Sistema.Data.Entities;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace Sistema.Services
{
    public interface ICommunicationService
    {
        Task<bool> SendEmailAsync(string to, string subject, string htmlBody);
        Task LogNotificationAsync(int? customerId, string type, string message, bool success);
    }

    public class CommunicationService : ICommunicationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<CommunicationService> _logger;
        private readonly SistemaDbContext _context;

        public CommunicationService(IConfiguration configuration, ILogger<CommunicationService> logger, SistemaDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@estabelecimento.pt";

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("Configurações de e-mail não encontradas");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPassword);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Ewellin Beauty"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"E-mail enviado com sucesso para {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail para {to}");
                return false;
            }
        }



        public async Task LogNotificationAsync(int? customerId, string type, string message, bool success)
        {
            try
            {
                var notification = new Notification
                {
                    Type = type,
                    Message = $"{message} - Cliente ID: {customerId} - Status: {(success ? "Enviado" : "Falhou")}",
                    Icon = GetIconForType(type),
                    Time = DateTime.Now.ToString("HH:mm"),
                    Link = "#",
                    CreatedAt = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar log de notificação");
            }
        }


        private string GetIconForType(string type)
        {
            return type.ToLower() switch
            {
                "email" => "fas fa-envelope",
                _ => "fas fa-bell"
            };
        }
    }
}
