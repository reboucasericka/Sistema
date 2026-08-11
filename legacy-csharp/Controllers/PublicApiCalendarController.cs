using Microsoft.AspNetCore.Mvc;
using Sistema.Services;
using System.Text.Json;

namespace Sistema.Controllers
{
    [ApiController]
    [Route("api/calendar")]
    public class PublicApiCalendarController : ControllerBase
    {
        private readonly IGoogleCalendarSyncService _googleCalendarSyncService;
        private readonly ILogger<PublicApiCalendarController> _logger;
        private readonly IConfiguration _configuration;

        public PublicApiCalendarController(
            IGoogleCalendarSyncService googleCalendarSyncService,
            ILogger<PublicApiCalendarController> logger,
            IConfiguration configuration)
        {
            _googleCalendarSyncService = googleCalendarSyncService;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Webhook para receber notificações do Google Calendar sobre mudanças em eventos
        /// </summary>
        /// <param name="data">Dados do evento atualizado do Google Calendar</param>
        /// <returns>Resposta de sucesso ou erro</returns>
        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveGoogleWebhook([FromBody] GoogleEventUpdate data)
        {
            try
            {
                _logger.LogInformation($"Webhook Google Calendar recebido: {JsonSerializer.Serialize(data)}");

                // Verificar se é uma notificação válida do Google Calendar
                if (data == null || string.IsNullOrEmpty(data.EventId))
                {
                    _logger.LogWarning("Webhook Google Calendar com dados inválidos");
                    return BadRequest("Dados inválidos");
                }

                // Verificar autenticação do webhook (opcional, mas recomendado)
                var webhookSecret = _configuration["GoogleCalendar:WebhookSecret"];
                if (!string.IsNullOrEmpty(webhookSecret))
                {
                    var receivedSecret = Request.Headers["X-Google-Webhook-Secret"].FirstOrDefault();
                    if (receivedSecret != webhookSecret)
                    {
                        _logger.LogWarning("Webhook Google Calendar com secret inválido");
                        return Unauthorized("Secret inválido");
                    }
                }

                // Processar a atualização do evento
                await ProcessGoogleEventUpdateAsync(data);

                return Ok(new { message = "Webhook processado com sucesso" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar webhook Google Calendar");
                return StatusCode(500, new { error = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Endpoint para sincronização manual com Google Calendar
        /// </summary>
        /// <param name="calendarId">ID do calendário (opcional, usa 'primary' por padrão)</param>
        /// <returns>Resultado da sincronização</returns>
        [HttpPost("sync")]
        public async Task<IActionResult> SyncFromGoogle([FromQuery] string? calendarId = null)
        {
            try
            {
                _logger.LogInformation($"Iniciando sincronização manual com Google Calendar: {calendarId ?? "primary"}");

                var targetCalendarId = calendarId ?? "primary";
                await _googleCalendarSyncService.SyncFromGoogleAsync(targetCalendarId);

                return Ok(new { 
                    message = "Sincronização concluída com sucesso",
                    calendarId = targetCalendarId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante sincronização manual com Google Calendar");
                return StatusCode(500, new { error = "Erro durante sincronização" });
            }
        }

        /// <summary>
        /// Endpoint para verificar status da autenticação com Google Calendar
        /// </summary>
        /// <returns>Status da autenticação</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetCalendarStatus()
        {
            try
            {
                var isAuthenticated = await _googleCalendarSyncService.IsAuthenticatedAsync();
                
                return Ok(new { 
                    authenticated = isAuthenticated,
                    timestamp = DateTime.UtcNow,
                    service = "Google Calendar API"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status do Google Calendar");
                return StatusCode(500, new { error = "Erro ao verificar status" });
            }
        }

        /// <summary>
        /// Endpoint para criar/atualizar evento específico no Google Calendar
        /// </summary>
        /// <param name="appointmentId">ID do agendamento</param>
        /// <returns>Resultado da operação</returns>
        [HttpPost("event/{appointmentId}")]
        public async Task<IActionResult> SyncAppointmentEvent(int appointmentId)
        {
            try
            {
                _logger.LogInformation($"Sincronizando agendamento {appointmentId} com Google Calendar");

                // Aqui você precisaria buscar o agendamento do banco de dados
                // Por simplicidade, vou retornar um erro indicando que precisa ser implementado
                return BadRequest(new { 
                    error = "Endpoint não implementado completamente",
                    message = "Necessário buscar agendamento do banco de dados antes da sincronização"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao sincronizar agendamento {appointmentId} com Google Calendar");
                return StatusCode(500, new { error = "Erro durante sincronização do agendamento" });
            }
        }

        private async Task ProcessGoogleEventUpdateAsync(GoogleEventUpdate data)
        {
            try
            {
                _logger.LogInformation($"Processando atualização do evento Google Calendar: {data.EventId}");

                // Aqui você implementaria a lógica para:
                // 1. Buscar o agendamento correspondente no banco de dados
                // 2. Atualizar os dados do agendamento com base nas mudanças do Google Calendar
                // 3. Salvar as alterações no banco de dados
                // 4. Enviar notificações se necessário

                // Exemplo de implementação:
                /*
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.GoogleEventId == data.EventId);
                
                if (appointment != null)
                {
                    // Atualizar dados do agendamento baseado no evento Google
                    if (data.StartTime.HasValue)
                        appointment.StartTime = data.StartTime.Value;
                    
                    if (data.EndTime.HasValue)
                        appointment.EndTime = data.EndTime.Value;
                    
                    appointment.LastSyncedAt = DateTime.UtcNow;
                    
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Agendamento {appointment.AppointmentId} atualizado via webhook Google Calendar");
                }
                else
                {
                    _logger.LogWarning($"Agendamento não encontrado para evento Google Calendar: {data.EventId}");
                }
                */

                _logger.LogInformation($"Processamento do webhook Google Calendar concluído: {data.EventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar atualização do evento Google Calendar: {data.EventId}");
                throw;
            }
        }
    }

    /// <summary>
    /// Modelo para dados de atualização de evento do Google Calendar
    /// </summary>
    public class GoogleEventUpdate
    {
        public string EventId { get; set; } = string.Empty;
        public string CalendarId { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime Updated { get; set; }
        public string? Action { get; set; } // "created", "updated", "deleted"
    }
}
