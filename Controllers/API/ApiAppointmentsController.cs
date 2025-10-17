using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models;
using Sistema.Services;
using Microsoft.Extensions.Logging;

namespace Sistema.Controllers.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiAppointmentsController : ControllerBase
    {
        private readonly SistemaDbContext _context;
        private readonly IGoogleCalendarSyncService _calendarService;
        private readonly ILogger<ApiAppointmentsController> _logger;

        public ApiAppointmentsController(
            SistemaDbContext context,
            IGoogleCalendarSyncService calendarService,
            ILogger<ApiAppointmentsController> logger)
        {
            _context = context;
            _calendarService = calendarService;
            _logger = logger;
        }

        // ============================
        // Criar agendamento via app
        // ============================
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Appointments.Add(model);
                await _context.SaveChangesAsync();

                // Sincronizar com Google Calendar
                try
                {
                    await _calendarService.CreateOrUpdateEventAsync(model);
                    _logger.LogInformation($"Evento Google Calendar criado para agendamento interno ID {model.AppointmentId}");
                }
                catch (Exception calendarEx)
                {
                    _logger.LogError(calendarEx, $"Erro ao sincronizar evento com Google Calendar para agendamento {model.AppointmentId}");
                }

                return Ok(new { 
                    success = true, 
                    message = "Agendamento criado e sincronizado com o Google Calendar.",
                    appointmentId = model.AppointmentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar agendamento via API.");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ============================
        // Atualizar agendamento via app
        // ============================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment model)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Service)
                .Include(a => a.Professional)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound(new { success = false, message = "Agendamento não encontrado." });

            try
            {
                // Atualizar propriedades
                appointment.StartTime = model.StartTime;
                appointment.EndTime = model.EndTime;
                appointment.Status = model.Status;
                appointment.Notes = model.Notes;
                appointment.ProfessionalId = model.ProfessionalId;
                appointment.ServiceId = model.ServiceId;
                appointment.CustomerId = model.CustomerId;

                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();

                // Sincronizar com Google Calendar
                try
                {
                    await _calendarService.CreateOrUpdateEventAsync(appointment);
                    _logger.LogInformation($"Evento Google Calendar atualizado para agendamento interno ID {appointment.AppointmentId}");
                }
                catch (Exception calendarEx)
                {
                    _logger.LogError(calendarEx, $"Erro ao sincronizar evento com Google Calendar para agendamento {appointment.AppointmentId}");
                }

                return Ok(new { 
                    success = true, 
                    message = "Agendamento atualizado e sincronizado com o Google Calendar.",
                    appointmentId = appointment.AppointmentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar agendamento via API.");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ============================
        // Cancelar agendamento via app
        // ============================
        [HttpDelete("cancel/{id}")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound(new { success = false, message = "Agendamento não encontrado." });

            try
            {
                // Remover evento do Google Calendar antes de deletar
                try
                {
                    if (!string.IsNullOrEmpty(appointment.GoogleEventId))
                    {
                        await _calendarService.DeleteEventAsync(appointment.GoogleEventId);
                        _logger.LogInformation($"Evento Google Calendar removido para agendamento interno ID {appointment.AppointmentId}");
                    }
                }
                catch (Exception calendarEx)
                {
                    _logger.LogError(calendarEx, $"Erro ao remover evento do Google Calendar para agendamento {appointment.AppointmentId}");
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Agendamento interno ID {appointment.AppointmentId} cancelado e removido do Google Calendar.");
                return Ok(new { 
                    success = true, 
                    message = "Agendamento cancelado e removido do Google Calendar.",
                    appointmentId = appointment.AppointmentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar agendamento via API.");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ============================
        // Listar agendamentos do profissional
        // ============================
        [HttpGet("professional/{professionalId}")]
        public async Task<IActionResult> GetAppointmentsByProfessional(int professionalId)
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .Where(a => a.ProfessionalId == professionalId)
                    .OrderBy(a => a.StartTime)
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    appointments = appointments,
                    count = appointments.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar agendamentos do profissional.");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ============================
        // Obter agendamento específico
        // ============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Service)
                    .Include(a => a.Professional)
                    .FirstOrDefaultAsync(a => a.AppointmentId == id);

                if (appointment == null)
                    return NotFound(new { success = false, message = "Agendamento não encontrado." });

                return Ok(new { 
                    success = true, 
                    appointment = appointment
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar agendamento.");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
