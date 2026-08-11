using Microsoft.AspNetCore.SignalR;
using Sistema.Data.Entities;
using System.Text.Json;

namespace Sistema.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string message, string type = "info", string userId = null);
        Task SendCashRegisterNotificationAsync(string message, decimal? amount = null);
        Task SendAppointmentNotificationAsync(string message, int appointmentId);
        Task SendFinancialNotificationAsync(string message, decimal amount);
        Task SendBookingConfirmationAsync(Appointment appointment);
        Task SendBookingCancelledAsync(Appointment appointment);
        Task SendBookingRescheduledAsync(Appointment appointment);
        string GenerateBookingEmail(Appointment appointment);
        string GenerateBookingCancelledEmail(Appointment appointment);
        string GenerateBookingRescheduledEmail(Appointment appointment);
        Task SendTawkNotificationAsync(string message);
        Task SendEmailAsync(string email, string subject, string body);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;
        private readonly IEmailService _emailService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NotificationService(
            IHubContext<NotificationHub> hubContext, 
            ILogger<NotificationService> logger,
            IEmailService emailService,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _hubContext = hubContext;
            _logger = logger;
            _emailService = emailService;
            _httpClient = httpClient;
            _configuration = configuration;
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

        public async Task SendBookingConfirmationAsync(Appointment appointment)
        {
            try
            {
                if (string.IsNullOrEmpty(appointment.Customer.Email))
                {
                    _logger.LogWarning($"Cliente {appointment.Customer.Name} não possui e-mail para envio de confirmação");
                    return;
                }

                string subject = "Confirmação do seu agendamento – Ewellin Jordão";
                string htmlBody = GenerateBookingEmail(appointment);

                await _emailService.SendEmailAsync(appointment.Customer.Email, subject, htmlBody);

                // Log da notificação
                await SendAppointmentNotificationAsync(
                    $"E-mail de confirmação enviado para {appointment.Customer.Name}", 
                    appointment.AppointmentId);

                _logger.LogInformation($"E-mail de confirmação enviado para {appointment.Customer.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail de confirmação para {appointment.Customer.Email}");
                throw;
            }
        }

        public string GenerateBookingEmail(Appointment appointment)
        {
            try
            {
                // Por enquanto, usar o template simples
                // TODO: Implementar renderização do template Razor quando necessário
                return GenerateSimpleBookingEmail(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar template de e-mail");
                // Fallback para template simples em caso de erro
                return GenerateSimpleBookingEmail(appointment);
            }
        }

        public async Task SendBookingCancelledAsync(Appointment appointment)
        {
            try
            {
                if (string.IsNullOrEmpty(appointment.Customer.Email))
                {
                    _logger.LogWarning($"Cliente {appointment.Customer.Name} não possui e-mail para envio de cancelamento");
                    return;
                }

                string subject = "O seu agendamento foi cancelado – Ewellin Jordão";
                string htmlBody = GenerateBookingCancelledEmail(appointment);

                await _emailService.SendEmailAsync(appointment.Customer.Email, subject, htmlBody);

                // Log da notificação
                await SendAppointmentNotificationAsync(
                    $"E-mail de cancelamento enviado para {appointment.Customer.Name}", 
                    appointment.AppointmentId);

                // Enviar notificação para Tawk.to
                await SendTawkNotificationAsync($"❌ Agendamento cancelado: {appointment.Service.Name} - {appointment.StartTime:dd/MM/yyyy HH:mm}");

                _logger.LogInformation($"E-mail de cancelamento enviado para {appointment.Customer.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail de cancelamento para {appointment.Customer.Email}");
                throw;
            }
        }

        public async Task SendBookingRescheduledAsync(Appointment appointment)
        {
            try
            {
                if (string.IsNullOrEmpty(appointment.Customer.Email))
                {
                    _logger.LogWarning($"Cliente {appointment.Customer.Name} não possui e-mail para envio de reagendamento");
                    return;
                }

                string subject = "O seu agendamento foi reagendado – Ewellin Jordão";
                string htmlBody = GenerateBookingRescheduledEmail(appointment);

                await _emailService.SendEmailAsync(appointment.Customer.Email, subject, htmlBody);

                // Log da notificação
                await SendAppointmentNotificationAsync(
                    $"E-mail de reagendamento enviado para {appointment.Customer.Name}", 
                    appointment.AppointmentId);

                // Enviar notificação para Tawk.to
                await SendTawkNotificationAsync($"🔄 Agendamento reagendado: {appointment.Service.Name} - novo horário {appointment.StartTime:dd/MM/yyyy HH:mm}");

                _logger.LogInformation($"E-mail de reagendamento enviado para {appointment.Customer.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail de reagendamento para {appointment.Customer.Email}");
                throw;
            }
        }

        public string GenerateBookingCancelledEmail(Appointment appointment)
        {
            try
            {
                // Por enquanto, usar o template simples
                // TODO: Implementar renderização do template Razor quando necessário
                return GenerateSimpleBookingCancelledEmail(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar template de e-mail de cancelamento");
                // Fallback para template simples em caso de erro
                return GenerateSimpleBookingCancelledEmail(appointment);
            }
        }

        public string GenerateBookingRescheduledEmail(Appointment appointment)
        {
            try
            {
                // Por enquanto, usar o template simples
                // TODO: Implementar renderização do template Razor quando necessário
                return GenerateSimpleBookingRescheduledEmail(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar template de e-mail de reagendamento");
                // Fallback para template simples em caso de erro
                return GenerateSimpleBookingRescheduledEmail(appointment);
            }
        }

        public async Task SendTawkNotificationAsync(string message)
        {
            try
            {
                string webhookUrl = _configuration["TawkTo:WebhookUrl"];
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    _logger.LogWarning("Tawk.to webhook URL não configurada");
                    return;
                }

                var payload = new { text = message };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(webhookUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Notificação Tawk.to enviada: {message}");
                }
                else
                {
                    _logger.LogWarning($"Falha ao enviar notificação Tawk.to: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar notificação para Tawk.to");
                // Não falha o processo principal se o Tawk.to falhar
            }
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            try
            {
                await _emailService.SendEmailAsync(email, subject, body);
                _logger.LogInformation($"E-mail enviado para {email}: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar e-mail para {email}");
                throw;
            }
        }

        private string GenerateSimpleBookingEmail(Appointment appointment)
        {
            var culture = new System.Globalization.CultureInfo("pt-PT");
            var dataHora = appointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture);
            var valor = appointment.Service.Price.ToString("C2", culture);

            return $@"
                <!DOCTYPE html>
                <html lang='pt-PT'>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>Confirmação do seu agendamento – Ewellin Jordão</title>
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
                            background: linear-gradient(135deg, #f16e00 0%, #ff8c42 100%);
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
                            border-left: 4px solid #f16e00;
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
                        .cta-section {{
                            text-align: center;
                            margin: 30px 0;
                        }}
                        .cta-button {{
                            display: inline-block;
                            background: linear-gradient(135deg, #f16e00 0%, #ff8c42 100%);
                            color: white;
                            padding: 15px 30px;
                            text-decoration: none;
                            border-radius: 25px;
                            font-weight: 600;
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
                            color: #f16e00;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Agendamento Confirmado!</h1>
                            <p>O seu compromisso foi marcado com sucesso</p>
                        </div>
                        <div class='content'>
                            <div class='greeting'>
                                Olá <strong>{appointment.Customer.Name}</strong>,
                            </div>
                            <div class='message'>
                                O seu agendamento foi confirmado com sucesso! Estamos ansiosos para recebê-lo(a) no nosso espaço.
                            </div>
                            <div class='appointment-details'>
                                <div class='detail-row'>
                                    <span class='detail-icon'>📅</span>
                                    <span class='detail-label'>Data e Hora:</span>
                                    <span class='detail-value'>{dataHora}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-icon'>💇</span>
                                    <span class='detail-label'>Serviço:</span>
                                    <span class='detail-value'>{appointment.Service.Name}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-icon'>👩‍🎨</span>
                                    <span class='detail-label'>Profissional:</span>
                                    <span class='detail-value'>{appointment.Professional.Name}</span>
                                </div>
                                <div class='detail-row'>
                                    <span class='detail-icon'>💶</span>
                                    <span class='detail-label'>Valor:</span>
                                    <span class='detail-value'>{valor}</span>
                                </div>
                            </div>
                            <div class='cta-section'>
                                <p class='message'>
                                    Clique no botão abaixo para visualizar ou gerir o seu agendamento:
                                </p>
                                <a href='https://localhost:7036/minhaarea/meuscompromissos' class='cta-button'>
                                    Ver Meus Agendamentos
                                </a>
                            </div>
                            <div class='message'>
                                <strong>Lembrete importante:</strong> Por favor, chegue 10 minutos antes do horário marcado. 
                                Em caso de necessidade de alteração ou cancelamento, entre em contacto connosco com pelo menos 24 horas de antecedência.
                            </div>
                            <div class='signature'>
                                <p>Agradecemos a sua preferência.</p>
                                <p>Com carinho,<br><strong>Equipa Ewellin Jordão</strong></p>
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
        }

        private string GenerateSimpleBookingCancelledEmail(Appointment appointment)
        {
            var culture = new System.Globalization.CultureInfo("pt-PT");
            var dataHora = appointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture);

            return $@"
                <!DOCTYPE html>
                <html lang='pt-PT'>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>O seu agendamento foi cancelado – Ewellin Jordão</title>
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
                            background: linear-gradient(135deg, #dc3545 0%, #e74c3c 100%);
                            color: white;
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 600;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .appointment-details {{
                            background-color: #f8f9fa;
                            border-radius: 8px;
                            padding: 25px;
                            margin: 25px 0;
                            border-left: 4px solid #dc3545;
                        }}
                        .footer {{
                            background-color: #f8f9fa;
                            padding: 25px 30px;
                            text-align: center;
                            color: #666;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Agendamento Cancelado</h1>
                            <p>O seu compromisso foi cancelado conforme solicitado</p>
                        </div>
                        <div class='content'>
                            <p>Olá <strong>{appointment.Customer.Name}</strong>,</p>
                            <p>Informamos que o seu agendamento foi cancelado conforme o seu pedido.</p>
                            <div class='appointment-details'>
                                <p><strong>💇 Serviço:</strong> {appointment.Service.Name}</p>
                                <p><strong>👩‍🎨 Profissional:</strong> {appointment.Professional.Name}</p>
                                <p><strong>❌ Data e hora original:</strong> {dataHora}</p>
                            </div>
                            <p>Se desejar reagendar, pode fazê-lo facilmente no seu painel.</p>
                            <p><strong>Esperamos vê-la novamente em breve! 💖</strong></p>
                            <p>Com carinho,<br><strong>Equipa Ewellin Jordão</strong></p>
                        </div>
                        <div class='footer'>
                            <p><strong>© 2025 Ewellin Jordão</strong> | Coimbra, Portugal</p>
                            <p>Este é um e-mail automático, por favor não responda.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
        }

        private string GenerateSimpleBookingRescheduledEmail(Appointment appointment)
        {
            var culture = new System.Globalization.CultureInfo("pt-PT");
            var dataHora = appointment.StartTime.ToString("dd/MM/yyyy HH:mm", culture);
            var valor = appointment.Service.Price.ToString("C2", culture);

            return $@"
                <!DOCTYPE html>
                <html lang='pt-PT'>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>O seu agendamento foi reagendado – Ewellin Jordão</title>
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
                            background: linear-gradient(135deg, #28a745 0%, #20c997 100%);
                            color: white;
                            padding: 40px 30px;
                            text-align: center;
                        }}
                        .header h1 {{
                            margin: 0;
                            font-size: 28px;
                            font-weight: 600;
                        }}
                        .content {{
                            padding: 40px 30px;
                        }}
                        .appointment-details {{
                            background-color: #f8f9fa;
                            border-radius: 8px;
                            padding: 25px;
                            margin: 25px 0;
                            border-left: 4px solid #28a745;
                        }}
                        .footer {{
                            background-color: #f8f9fa;
                            padding: 25px 30px;
                            text-align: center;
                            color: #666;
                            font-size: 14px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Agendamento Reagendado!</h1>
                            <p>O seu compromisso foi atualizado com sucesso</p>
                        </div>
                        <div class='content'>
                            <p>Olá <strong>{appointment.Customer.Name}</strong>,</p>
                            <p>O seu agendamento foi reagendado com sucesso! A sua marcação anterior foi atualizada no sistema.</p>
                            <div class='appointment-details'>
                                <p><strong>🗓️ Novo horário:</strong> {dataHora}</p>
                                <p><strong>💇 Serviço:</strong> {appointment.Service.Name}</p>
                                <p><strong>👩‍🎨 Profissional:</strong> {appointment.Professional.Name}</p>
                                <p><strong>💶 Valor:</strong> {valor}</p>
                            </div>
                            <p>Pode visualizar os detalhes ou cancelar, se necessário, no seu painel.</p>
                            <p>Com carinho,<br><strong>Equipa Ewellin Jordão</strong> 💖</p>
                        </div>
                        <div class='footer'>
                            <p><strong>© 2025 Ewellin Jordão</strong> | Coimbra, Portugal</p>
                            <p>Este é um e-mail automático, por favor não responda.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";
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
