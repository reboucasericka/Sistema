using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using System.Text.Json;

namespace Sistema.Services
{
    public interface IGoogleCalendarSyncService
    {
        Task<string> CreateOrUpdateEventAsync(Appointment appointment);
        Task DeleteEventAsync(string eventId);
        Task SyncFromGoogleAsync(string calendarId);
        Task<CalendarService> GetServiceAsync();
        Task<bool> IsAuthenticatedAsync();
    }

    public class GoogleCalendarSyncService : IGoogleCalendarSyncService
    {
        private readonly SistemaDbContext _context;
        private readonly ILogger<GoogleCalendarSyncService> _logger;
        private readonly IConfiguration _configuration;
        private CalendarService? _calendarService;
        private readonly string _calendarId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _refreshToken;

        public GoogleCalendarSyncService(
            SistemaDbContext context,
            ILogger<GoogleCalendarSyncService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;

            // Configurações do Google Calendar
            _calendarId = _configuration["GoogleCalendar:CalendarId"] ?? "primary";
            _clientId = _configuration["GoogleCalendar:ClientId"] ?? "";
            _clientSecret = _configuration["GoogleCalendar:ClientSecret"] ?? "";
            _refreshToken = _configuration["GoogleCalendar:RefreshToken"] ?? "";
        }

        public async Task<CalendarService> GetServiceAsync()
        {
            if (_calendarService != null)
                return _calendarService;

            try
            {
                // Para aplicações server-side, usar Service Account ou Refresh Token
                // Esta implementação usa refresh token para autenticação
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = _clientId,
                        ClientSecret = _clientSecret
                    },
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("EwellinJordao.Calendar.Auth.Store", true)
                ).Result;

                _calendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Ewellin Jordão - Sistema de Agendamentos"
                });

                _logger.LogInformation("Google Calendar Service inicializado com sucesso");
                return _calendarService;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inicializar Google Calendar Service");
                throw;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var service = await GetServiceAsync();
                var calendar = await service.Calendars.Get(_calendarId).ExecuteAsync();
                return calendar != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar autenticação do Google Calendar");
                return false;
            }
        }

        public async Task<string> CreateOrUpdateEventAsync(Appointment appointment)
        {
            try
            {
                var service = await GetServiceAsync();
                
                // Verificar se já existe um evento no Google Calendar
                Event? existingEvent = null;
                if (!string.IsNullOrEmpty(appointment.GoogleEventId))
                {
                    try
                    {
                        existingEvent = await service.Events.Get(_calendarId, appointment.GoogleEventId).ExecuteAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Evento Google Calendar não encontrado: {appointment.GoogleEventId}");
                    }
                }

                var eventData = CreateEventFromAppointment(appointment);

                Event createdEvent;
                if (existingEvent != null)
                {
                    // Atualizar evento existente
                    createdEvent = await service.Events.Update(eventData, _calendarId, appointment.GoogleEventId).ExecuteAsync();
                    _logger.LogInformation($"Evento Google Calendar atualizado: {createdEvent.Id} para agendamento {appointment.AppointmentId}");
                }
                else
                {
                    // Criar novo evento
                    createdEvent = await service.Events.Insert(eventData, _calendarId).ExecuteAsync();
                    _logger.LogInformation($"Novo evento Google Calendar criado: {createdEvent.Id} para agendamento {appointment.AppointmentId}");
                }

                // Atualizar o agendamento com o ID do evento Google
                appointment.GoogleEventId = createdEvent.Id;
                
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();

                return createdEvent.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao criar/atualizar evento Google Calendar para agendamento {appointment.AppointmentId}");
                throw;
            }
        }

        public async Task DeleteEventAsync(string eventId)
        {
            try
            {
                if (string.IsNullOrEmpty(eventId))
                    return;

                var service = await GetServiceAsync();
                await service.Events.Delete(_calendarId, eventId).ExecuteAsync();
                
                _logger.LogInformation($"Evento Google Calendar removido: {eventId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover evento Google Calendar: {eventId}");
                throw;
            }
        }

        public async Task SyncFromGoogleAsync(string calendarId)
        {
            try
            {
                var service = await GetServiceAsync();
                var request = service.Events.List(calendarId);
                request.TimeMin = DateTime.UtcNow.AddDays(-30); // Últimos 30 dias
                request.TimeMax = DateTime.UtcNow.AddDays(90);  // Próximos 90 dias
                request.SingleEvents = true;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                var events = await request.ExecuteAsync();
                
                _logger.LogInformation($"Sincronizando {events.Items?.Count ?? 0} eventos do Google Calendar");

                foreach (var googleEvent in events.Items ?? new List<Event>())
                {
                    await ProcessGoogleEventAsync(googleEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar eventos do Google Calendar");
                throw;
            }
        }

        private Event CreateEventFromAppointment(Appointment appointment)
        {
            var culture = new System.Globalization.CultureInfo("pt-PT");
            var startTime = appointment.StartTime;
            var endTime = appointment.EndTime; // Usar EndTime diretamente

            var eventData = new Event
            {
                Summary = $"💇 {appointment.Service?.Name} - {appointment.Customer?.Name}",
                Description = $"Agendamento Ewellin Jordão\n\n" +
                             $"👤 Cliente: {appointment.Customer?.Name}\n" +
                             $"💇 Serviço: {appointment.Service?.Name}\n" +
                             $"👩‍🎨 Profissional: {appointment.Professional?.Name}\n" +
                             $"💰 Valor: {appointment.Service?.Price.ToString("C2", culture)}\n" +
                             $"📞 Telefone: {appointment.Customer?.Phone}\n" +
                             $"📧 E-mail: {appointment.Customer?.Email}\n\n" +
                             $"ID do Sistema: {appointment.AppointmentId}",
                Start = new EventDateTime
                {
                    DateTime = startTime,
                    TimeZone = "Europe/Lisbon"
                },
                End = new EventDateTime
                {
                    DateTime = endTime,
                    TimeZone = "Europe/Lisbon"
                },
                Location = "Ewellin Jordão - Coimbra, Portugal",
                ColorId = GetColorIdForStatus(appointment.Status),
                Reminders = new Event.RemindersData
                {
                    UseDefault = false,
                    Overrides = new List<EventReminder>
                    {
                        new EventReminder { Method = "email", Minutes = 24 * 60 }, // 24 horas
                        new EventReminder { Method = "popup", Minutes = 2 * 60 }   // 2 horas
                    }
                },
                ExtendedProperties = new Event.ExtendedPropertiesData
                {
                    Private__ = new Dictionary<string, string>
                    {
                        { "appointmentId", appointment.AppointmentId.ToString() },
                        { "customerId", appointment.CustomerId.ToString() },
                        { "serviceId", appointment.ServiceId.ToString() },
                        { "professionalId", appointment.ProfessionalId.ToString() }
                    }
                }
            };

            return eventData;
        }

        private string GetColorIdForStatus(string? status)
        {
            return status?.ToLower() switch
            {
                "agendado" or "scheduled" => "2", // Verde
                "confirmado" or "confirmed" => "5", // Amarelo
                "concluído" or "completed" => "10", // Verde escuro
                "cancelado" or "cancelled" => "11", // Vermelho
                _ => "1" // Azul (padrão)
            };
        }

        private async Task ProcessGoogleEventAsync(Event googleEvent)
        {
            try
            {
                // Verificar se o evento tem o ID do agendamento nos metadados
                var appointmentIdStr = googleEvent.ExtendedProperties?.Private__?.ContainsKey("appointmentId") == true 
                    ? googleEvent.ExtendedProperties.Private__["appointmentId"] 
                    : null;
                if (string.IsNullOrEmpty(appointmentIdStr) || !int.TryParse(appointmentIdStr, out int appointmentId))
                {
                    _logger.LogWarning($"Evento Google Calendar sem ID de agendamento: {googleEvent.Id}");
                    return;
                }

                var appointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                {
                    _logger.LogWarning($"Agendamento não encontrado para evento Google Calendar: {appointmentId}");
                    return;
                }

                // Verificar se houve alterações no evento
                var googleStartTime = googleEvent.Start?.DateTime;
                var googleEndTime = googleEvent.End?.DateTime;

                if (googleStartTime.HasValue && googleStartTime.Value != appointment.StartTime)
                {
                    appointment.StartTime = googleStartTime.Value;
                    if (googleEndTime.HasValue)
                        appointment.EndTime = googleEndTime.Value;
                    
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Agendamento {appointmentId} atualizado via Google Calendar: {googleStartTime.Value}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar evento Google Calendar: {googleEvent.Id}");
            }
        }
    }

}
