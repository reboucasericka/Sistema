using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Services
{
    public interface IAppointmentReminderService
    {
        Task ProcessRemindersAsync();
    }

    public class AppointmentReminderService : BackgroundService, IAppointmentReminderService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderService> _logger;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _period = TimeSpan.FromHours(1); // Executa a cada hora

        public AppointmentReminderService(
            IServiceProvider serviceProvider,
            ILogger<AppointmentReminderService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentReminderService iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante o processamento de lembretes");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("AppointmentReminderService parado");
        }

        public async Task ProcessRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SistemaDbContext>();
            // var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>(); // Moved to API Business Services

            try
            {
                var now = DateTime.UtcNow;
                
                // Lembretes de 24 horas (entre 24h e 25h antes)
                var reminder24hStart = now.AddHours(24);
                var reminder24hEnd = now.AddHours(25);
                
                // Lembretes de 2 horas (entre 2h e 3h antes)
                var reminder2hStart = now.AddHours(2);
                var reminder2hEnd = now.AddHours(3);

                // Buscar agendamentos para lembrete de 24h
                var appointments24h = await GetAppointmentsForReminderAsync(
                    context, reminder24hStart, reminder24hEnd, ReminderType.TwentyFourHours);

                // Buscar agendamentos para lembrete de 2h
                var appointments2h = await GetAppointmentsForReminderAsync(
                    context, reminder2hStart, reminder2hEnd, ReminderType.TwoHours);

                _logger.LogInformation($"Processando {appointments24h.Count} lembretes de 24h e {appointments2h.Count} lembretes de 2h");

                // Processar lembretes de 24h
                foreach (var appointment in appointments24h)
                {
                    // await SendReminderAsync(appointment, ReminderType.TwentyFourHours, notificationService, context); // Moved to API Business Services
                }

                // Processar lembretes de 2h
                foreach (var appointment in appointments2h)
                {
                    // await SendReminderAsync(appointment, ReminderType.TwoHours, notificationService, context); // Moved to API Business Services
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar lembretes de agendamentos");
            }
        }

        private async Task<List<Appointment>> GetAppointmentsForReminderAsync(
            SistemaDbContext context, 
            DateTime startTime, 
            DateTime endTime, 
            ReminderType reminderType)
        {
            var lastReminderSent = reminderType == ReminderType.TwentyFourHours 
                ? DateTime.UtcNow.AddHours(-1) // Não enviar novamente se já foi enviado na última hora
                : DateTime.UtcNow.AddMinutes(-30); // Para 2h, não enviar novamente se já foi enviado nos últimos 30 min

            return await context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Service)
                .Include(a => a.Professional)
                .Where(a => a.StartTime >= startTime && 
                           a.StartTime <= endTime &&
                           a.Status != null && 
                           (a.Status.ToLower() == "agendado" || a.Status.ToLower() == "scheduled" || 
                            a.Status.ToLower() == "confirmado" || a.Status.ToLower() == "confirmed") &&
                           !a.ReminderSent)
                .ToListAsync();
        }

        private async Task SendReminderAsync(
            Appointment appointment, 
            ReminderType reminderType, 
            // Notification service moved to API Business Services
            SistemaDbContext context)
        {
            try
            {
                var culture = new System.Globalization.CultureInfo("pt-PT");
                var dataHora = appointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture);

                var subject = "Lembrete do seu agendamento – Ewellin Jordão";
                var hoursText = reminderType == ReminderType.TwentyFourHours ? "24 horas" : "2 horas";
                
                var htmlBody = $@"
                    <!DOCTYPE html>
                    <html lang='pt-PT'>
                    <head>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                        <title>{subject}</title>
                        <style>
                            body {{
                                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                                line-height: 1.6;
                                color: #333;
                                margin: 0;
                                padding: 0;
                                background-color: #f5f5f5;
                            }}
                            .container {{
                                max-width: 600px;
                                margin: 0 auto;
                                background-color: #ffffff;
                                border-radius: 10px;
                                overflow: hidden;
                                box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
                            }}
                            .header {{
                                background: linear-gradient(135deg, #C97D5D 0%, #D9A791 100%);
                                color: white;
                                padding: 40px 30px;
                                text-align: center;
                            }}
                            .header h1 {{
                                margin: 0;
                                font-size: 28px;
                                font-weight: 600;
                            }}
                            .header p {{
                                margin: 10px 0 0 0;
                                font-size: 16px;
                                opacity: 0.9;
                            }}
                            .content {{
                                padding: 40px 30px;
                            }}
                            .greeting {{
                                font-size: 18px;
                                margin-bottom: 20px;
                                color: #333;
                            }}
                            .appointment-details {{
                                background-color: #f8f9fa;
                                border-radius: 8px;
                                padding: 25px;
                                margin: 25px 0;
                                border-left: 4px solid #C97D5D;
                            }}
                            .detail-row {{
                                display: flex;
                                align-items: center;
                                margin-bottom: 15px;
                                font-size: 16px;
                            }}
                            .detail-row:last-child {{
                                margin-bottom: 0;
                            }}
                            .detail-icon {{
                                font-size: 20px;
                                margin-right: 15px;
                                width: 25px;
                                text-align: center;
                            }}
                            .detail-label {{
                                font-weight: 600;
                                color: #555;
                                min-width: 120px;
                            }}
                            .detail-value {{
                                color: #333;
                                flex: 1;
                            }}
                            .reminder-info {{
                                background: linear-gradient(135deg, #fff3cd 0%, #ffeaa7 100%);
                                border: 1px solid #ffeaa7;
                                border-radius: 8px;
                                padding: 20px;
                                margin: 20px 0;
                                text-align: center;
                            }}
                            .reminder-info h3 {{
                                margin: 0 0 10px 0;
                                color: #856404;
                                font-size: 18px;
                            }}
                            .reminder-info p {{
                                margin: 0;
                                color: #856404;
                                font-size: 16px;
                            }}
                            .message {{
                                font-size: 16px;
                                line-height: 1.6;
                                margin: 20px 0;
                                color: #555;
                            }}
                            .signature {{
                                margin-top: 30px;
                                padding-top: 20px;
                                border-top: 1px solid #eee;
                                text-align: center;
                                color: #666;
                            }}
                            .footer {{
                                background-color: #f8f9fa;
                                padding: 25px 30px;
                                text-align: center;
                                color: #666;
                                font-size: 14px;
                                border-top: 1px solid #eee;
                            }}
                            .footer p {{
                                margin: 5px 0;
                            }}
                            .footer .company-name {{
                                font-weight: 600;
                                color: #C97D5D;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>⏰ Lembrete de Agendamento</h1>
                                <p>O seu compromisso está próximo!</p>
                            </div>
                            <div class='content'>
                                <div class='greeting'>
                                    Olá <strong>{appointment.Customer.Name}</strong>,
                                </div>
                                
                                <div class='reminder-info'>
                                    <h3>🔔 Lembrete de {hoursText}</h3>
                                    <p>O seu agendamento está marcado para {hoursText}!</p>
                                </div>

                                <div class='message'>
                                    Este é um lembrete do seu agendamento na Ewellin Jordão. Estamos ansiosos para recebê-la!
                                </div>

                                <div class='appointment-details'>
                                    <div class='detail-row'>
                                        <span class='detail-icon'>📅</span>
                                        <span class='detail-label'>Data e hora:</span>
                                        <span class='detail-value'>{dataHora}</span>
                                    </div>
                                    <div class='detail-row'>
                                        <span class='detail-icon'>💇</span>
                                        <span class='detail-label'>Serviço:</span>
                                        <span class='detail-value'>{appointment.Service?.Name}</span>
                                    </div>
                                    <div class='detail-row'>
                                        <span class='detail-icon'>👩‍🎨</span>
                                        <span class='detail-label'>Profissional:</span>
                                        <span class='detail-value'>{appointment.Professional?.Name}</span>
                                    </div>
                                </div>

                                <div class='message'>
                                    <strong>Lembrete importante:</strong> Por favor, chegue 10 minutos antes do horário marcado. 
                                    Em caso de necessidade de alteração ou cancelamento, entre em contacto connosco o mais rapidamente possível.
                                </div>

                                <div class='signature'>
                                    <p>Até breve!</p>
                                    <p>Com carinho,<br><strong>Equipa Ewellin Jordão</strong> 💖</p>
                                </div>
                            </div>
                            <div class='footer'>
                                <p><span class='company-name'>© 2025 Ewellin Jordão</span> | Coimbra, Portugal</p>
                                <p>Este é um e-mail automático, por favor não responda.</p>
                                <p>Para questões ou suporte, contacte-nos através dos nossos canais oficiais.</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                // Enviar e-mail
                if (!string.IsNullOrEmpty(appointment.Customer.Email))
                {
                    // await notificationService.SendEmailAsync(appointment.Customer.Email, subject, htmlBody); // Moved to API Business Services
                    _logger.LogInformation($"Lembrete de {hoursText} enviado por e-mail para {appointment.Customer.Email} - Agendamento {appointment.AppointmentId}");
                }

                // Enviar notificação Tawk.to se configurado
                var notifyTawk = _configuration.GetValue<bool>("AppointmentReminders:NotifyTawk", true);
                if (notifyTawk)
                {
                    var tawkMessage = $"⏰ Lembrete de {hoursText}: {appointment.Customer.Name} - {appointment.Service?.Name} às {dataHora}";
                    // await notificationService.SendTawkNotificationAsync(tawkMessage); // Moved to API Business Services
                    _logger.LogInformation($"Lembrete de {hoursText} enviado para Tawk.to - Agendamento {appointment.AppointmentId}");
                }

                // Atualizar ReminderSent
                appointment.ReminderSent = true;
                context.Appointments.Update(appointment);
                await context.SaveChangesAsync();

                _logger.LogInformation($"Lembrete de {hoursText} processado com sucesso para agendamento {appointment.AppointmentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar lembrete de {reminderType} para agendamento {appointment.AppointmentId}");
            }
        }
    }

    public enum ReminderType
    {
        TwentyFourHours,
        TwoHours
    }
}
