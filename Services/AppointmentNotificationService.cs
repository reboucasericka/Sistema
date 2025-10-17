using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using System.Globalization;

namespace Sistema.Services
{
    public interface IAppointmentNotificationService
    {
        Task SendAppointmentConfirmationAsync(Appointment appointment);
        Task SendAppointmentCancellationAsync(Appointment appointment);
        Task SendAppointmentReminderAsync(Appointment appointment);
    }

    public class AppointmentNotificationService : IAppointmentNotificationService
    {
        private readonly ICommunicationService _communicationService;
        private readonly ILogger<AppointmentNotificationService> _logger;
        private readonly SistemaDbContext _context;
        private readonly IConfiguration _configuration;

        public AppointmentNotificationService(
            ICommunicationService communicationService,
            ILogger<AppointmentNotificationService> logger,
            SistemaDbContext context,
            IConfiguration configuration)
        {
            _communicationService = communicationService;
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public async Task SendAppointmentConfirmationAsync(Appointment appointment)
        {
            try
            {
                // Buscar dados completos do agendamento
                var fullAppointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                if (fullAppointment?.Customer == null || fullAppointment.Service == null)
                {
                    _logger.LogWarning($"Dados incompletos para agendamento {appointment.AppointmentId}");
                    return;
                }

                var customer = fullAppointment.Customer;
                var service = fullAppointment.Service;
                var professional = fullAppointment.Professional;

                // Formatar data e hora em português europeu
                var culture = new CultureInfo("pt-PT");
                var dateTime = fullAppointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture);
                var price = service.Price.ToString("C", culture);

                // Mensagens em português europeu
                var emailSubject = $"Confirmação de Agendamento - {service.Name}";
                var emailBody = GenerateEmailBody(customer.Name, service.Name, professional?.Name, dateTime, price, fullAppointment.AppointmentId);

                // Enviar notificações
                var emailSent = await _communicationService.SendEmailAsync(customer.Email, emailSubject, emailBody);
                await _communicationService.LogNotificationAsync(customer.CustomerId, "Email", emailSubject, emailSent);

                _logger.LogInformation($"Notificações de confirmação enviadas para agendamento {appointment.AppointmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar notificações de confirmação para agendamento {appointment.AppointmentId}");
            }
        }

        public async Task SendAppointmentCancellationAsync(Appointment appointment)
        {
            try
            {
                // Buscar dados completos do agendamento
                var fullAppointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                if (fullAppointment?.Customer == null || fullAppointment.Service == null)
                {
                    _logger.LogWarning($"Dados incompletos para agendamento {appointment.AppointmentId}");
                    return;
                }

                var customer = fullAppointment.Customer;
                var service = fullAppointment.Service;
                var professional = fullAppointment.Professional;

                // Formatar data e hora em português europeu
                var culture = new CultureInfo("pt-PT");
                var dateTime = fullAppointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture);

                // Mensagens de cancelamento
                var emailSubject = $"Cancelamento de Agendamento - {service.Name}";
                var emailBody = GenerateCancellationEmailBody(customer.Name, service.Name, professional?.Name, dateTime);

                // Enviar notificações
                var emailSent = await _communicationService.SendEmailAsync(customer.Email, emailSubject, emailBody);
                await _communicationService.LogNotificationAsync(customer.CustomerId, "Email", emailSubject, emailSent);

                _logger.LogInformation($"Notificações de cancelamento enviadas para agendamento {appointment.AppointmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar notificações de cancelamento para agendamento {appointment.AppointmentId}");
            }
        }

        public async Task SendAppointmentReminderAsync(Appointment appointment)
        {
            try
            {
                // Buscar dados completos do agendamento
                var fullAppointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                if (fullAppointment?.Customer == null || fullAppointment.Service == null)
                {
                    _logger.LogWarning($"Dados incompletos para agendamento {appointment.AppointmentId}");
                    return;
                }

                var customer = fullAppointment.Customer;
                var service = fullAppointment.Service;
                var professional = fullAppointment.Professional;

                // Formatar data e hora em português europeu
                var culture = new CultureInfo("pt-PT");
                var dateTime = fullAppointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture);

                // Mensagens de lembrete
                var emailSubject = $"Lembrete de Agendamento - {service.Name}";
                var emailBody = GenerateReminderEmailBody(customer.Name, service.Name, professional?.Name, dateTime);

                // Enviar notificações
                var emailSent = await _communicationService.SendEmailAsync(customer.Email, emailSubject, emailBody);
                await _communicationService.LogNotificationAsync(customer.CustomerId, "Email", emailSubject, emailSent);

                _logger.LogInformation($"Notificações de lembrete enviadas para agendamento {appointment.AppointmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar notificações de lembrete para agendamento {appointment.AppointmentId}");
            }
        }

        private string GenerateEmailBody(string customerName, string serviceName, string? professionalName, string dateTime, string price, int appointmentId)
        {
            var firstName = customerName.Split(' ')[0];
            var cancelUrl = $"{_configuration["AppSettings:BaseUrl"]}/Public/PublicBooking/Cancel/{appointmentId}";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .appointment-details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .detail-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .detail-label {{ font-weight: bold; color: #667eea; }}
        .btn {{ display: inline-block; padding: 12px 24px; background: #dc3545; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Agendamento Confirmado!</h1>
            <p>Ewellin Beauty</p>
        </div>
        
        <div class='content'>
            <h2>Olá {firstName}!</h2>
            <p>O seu agendamento foi confirmado com sucesso. Aqui estão os detalhes:</p>
            
            <div class='appointment-details'>
                <div class='detail-row'>
                    <span class='detail-label'>📅 Data e Hora:</span>
                    <span>{dateTime}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>💆 Serviço:</span>
                    <span>{serviceName}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>👨‍⚕️ Profissional:</span>
                    <span>{professionalName ?? "A definir"}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>💰 Preço:</span>
                    <span>{price}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>📍 Local:</span>
                    <span>Ewellin Beauty<br>Rua da Beleza, 123, Centro, Lisboa</span>
                </div>
            </div>
            
            <p><strong>Importante:</strong> Chegue 10 minutos antes do horário agendado.</p>
            
            <a href='{cancelUrl}' class='btn'>Cancelar Agendamento</a>
            
            <div class='footer'>
                <p>Obrigada pela sua preferência! 💕</p>
                <p>Ewellin Beauty - Estética e Bem-Estar</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateCancellationEmailBody(string customerName, string serviceName, string? professionalName, string dateTime)
        {
            var firstName = customerName.Split(' ')[0];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #dc3545; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .appointment-details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .detail-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .detail-label {{ font-weight: bold; color: #dc3545; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>❌ Agendamento Cancelado</h1>
            <p>Ewellin Beauty</p>
        </div>
        
        <div class='content'>
            <h2>Olá {firstName}!</h2>
            <p>O seu agendamento foi cancelado com sucesso:</p>
            
            <div class='appointment-details'>
                <div class='detail-row'>
                    <span class='detail-label'>📅 Data e Hora:</span>
                    <span>{dateTime}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>💆 Serviço:</span>
                    <span>{serviceName}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>👨‍⚕️ Profissional:</span>
                    <span>{professionalName ?? "A definir"}</span>
                </div>
            </div>
            
            <p>Se precisar reagendar, entre em contacto connosco!</p>
            
            <div class='footer'>
                <p>Obrigada pela sua compreensão! 💕</p>
                <p>Ewellin Beauty - Estética e Bem-Estar</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GenerateReminderEmailBody(string customerName, string serviceName, string? professionalName, string dateTime)
        {
            var firstName = customerName.Split(' ')[0];

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #ffc107; color: #333; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .appointment-details {{ background: white; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .detail-row {{ display: flex; justify-content: space-between; margin: 10px 0; padding: 8px 0; border-bottom: 1px solid #eee; }}
        .detail-label {{ font-weight: bold; color: #ffc107; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏰ Lembrete de Agendamento</h1>
            <p>Ewellin Beauty</p>
        </div>
        
        <div class='content'>
            <h2>Olá {firstName}!</h2>
            <p>Lembramos que tem um agendamento:</p>
            
            <div class='appointment-details'>
                <div class='detail-row'>
                    <span class='detail-label'>📅 Data e Hora:</span>
                    <span>{dateTime}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>💆 Serviço:</span>
                    <span>{serviceName}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>👨‍⚕️ Profissional:</span>
                    <span>{professionalName ?? "A definir"}</span>
                </div>
                <div class='detail-row'>
                    <span class='detail-label'>📍 Local:</span>
                    <span>Ewellin Beauty<br>Rua da Beleza, 123, Centro, Lisboa</span>
                </div>
            </div>
            
            <p><strong>Importante:</strong> Chegue 10 minutos antes do horário agendado.</p>
            
            <div class='footer'>
                <p>Até breve! 💕</p>
                <p>Ewellin Beauty - Estética e Bem-Estar</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
