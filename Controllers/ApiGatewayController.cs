using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Api;
using System.Security.Claims;

namespace Sistema.Controllers
{
    [ApiController]
    [Route("gateway")]
    [Authorize]
    public class ApiGatewayController : ControllerBase
    {
        private readonly IApiAppointmentService _appointmentService;
        private readonly IApiUserService _userService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ApiGatewayController> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public ApiGatewayController(
            IApiAppointmentService appointmentService,
            IApiUserService userService,
            IMemoryCache cache,
            ILogger<ApiGatewayController> logger)
        {
            _appointmentService = appointmentService;
            _userService = userService;
            _cache = cache;
            _logger = logger;
        }

        #region Appointments Gateway

        [HttpGet("appointments")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AppointmentDto>>>> GetAppointments()
        {
            try
            {
                var cacheKey = "gateway_appointments_all";
                
                if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<AppointmentDto>>? cachedResult))
                {
                    _logger.LogInformation("Returning cached appointments");
                    return Ok(cachedResult);
                }

                var result = await _appointmentService.GetAppointmentsAsync();
                
                if (result.Success && result.Data != null)
                {
                    _cache.Set(cacheKey, result, _cacheExpiration);
                    _logger.LogInformation("Cached appointments for future requests");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAppointments gateway");
                return BadRequest(ApiResponse<IEnumerable<AppointmentDto>>.Fail($"Gateway error: {ex.Message}"));
            }
        }

        [HttpGet("appointments/{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetAppointment(int id)
        {
            try
            {
                var cacheKey = $"gateway_appointment_{id}";
                
                if (_cache.TryGetValue(cacheKey, out ApiResponse<AppointmentDto>? cachedResult))
                {
                    _logger.LogInformation($"Returning cached appointment {id}");
                    return Ok(cachedResult);
                }

                var result = await _appointmentService.GetAppointmentAsync(id);
                
                if (result.Success && result.Data != null)
                {
                    _cache.Set(cacheKey, result, _cacheExpiration);
                    _logger.LogInformation($"Cached appointment {id} for future requests");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetAppointment gateway for ID {id}");
                return BadRequest(ApiResponse<AppointmentDto>.Fail($"Gateway error: {ex.Message}"));
            }
        }

        [HttpPost("appointments")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> CreateAppointment([FromBody] AppointmentDto appointment)
        {
            try
            {
                _logger.LogInformation($"Creating appointment via gateway for client {appointment.ClientId}");
                
                var result = await _appointmentService.CreateAppointmentAsync(appointment);
                
                if (result.Success)
                {
                    // Invalidate cache
                    _cache.Remove("gateway_appointments_all");
                    _logger.LogInformation("Invalidated appointments cache after creation");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAppointment gateway");
                return BadRequest(ApiResponse<AppointmentDto>.Fail($"Gateway error: {ex.Message}"));
            }
        }

        [HttpPut("appointments/{id}")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> UpdateAppointment(int id, [FromBody] AppointmentDto appointment)
        {
            try
            {
                _logger.LogInformation($"Updating appointment {id} via gateway");
                
                var result = await _appointmentService.UpdateAppointmentAsync(id, appointment);
                
                if (result.Success)
                {
                    // Invalidate cache
                    _cache.Remove("gateway_appointments_all");
                    _cache.Remove($"gateway_appointment_{id}");
                    _logger.LogInformation("Invalidated appointments cache after update");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateAppointment gateway for ID {id}");
                return BadRequest(ApiResponse<AppointmentDto>.Fail($"Gateway error: {ex.Message}"));
            }
        }

        [HttpDelete("appointments/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAppointment(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting appointment {id} via gateway");
                
                var result = await _appointmentService.DeleteAppointmentAsync(id);
                
                if (result.Success)
                {
                    // Invalidate cache
                    _cache.Remove("gateway_appointments_all");
                    _cache.Remove($"gateway_appointment_{id}");
                    _logger.LogInformation("Invalidated appointments cache after deletion");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DeleteAppointment gateway for ID {id}");
                return BadRequest(ApiResponse<bool>.Fail($"Gateway error: {ex.Message}"));
            }
        }

        #endregion

        #region Users/Staff Gateway

        [HttpGet("staff")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProfessionalDto>>>> GetStaff()
        {
            try
            {
                var cacheKey = "gateway_staff_all";
                
                if (_cache.TryGetValue(cacheKey, out ApiResponse<IEnumerable<ProfessionalDto>>? cachedResult))
                {
                    _logger.LogInformation("Returning cached staff");
                    return Ok(cachedResult);
                }

                var result = await _userService.GetProfessionalsAsync();
                
                if (result.Success && result.Data != null)
                {
                    _cache.Set(cacheKey, result, _cacheExpiration);
                    _logger.LogInformation("Cached staff for future requests");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStaff gateway");
                return BadRequest(ApiResponse<IEnumerable<ProfessionalDto>>.Fail($"Gateway error: {ex.Message}"));
            }
        }

        [HttpGet("staff/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<ApiResponse<ProfessionalDto>>> GetStaffMember(int id)
        {
            try
            {
                var cacheKey = $"gateway_staff_{id}";
                
                if (_cache.TryGetValue(cacheKey, out ApiResponse<ProfessionalDto>? cachedResult))
                {
                    _logger.LogInformation($"Returning cached staff member {id}");
                    return Ok(cachedResult);
                }

                var result = await _userService.GetProfessionalAsync(id);
                
                if (result.Success && result.Data != null)
                {
                    _cache.Set(cacheKey, result, _cacheExpiration);
                    _logger.LogInformation($"Cached staff member {id} for future requests");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetStaffMember gateway for ID {id}");
                return BadRequest(ApiResponse<ProfessionalDto>.Fail($"Gateway error: {ex.Message}"));
            }
        }

        #endregion

        #region Health Check

        [HttpGet("health")]
        public ActionResult<object> HealthCheck()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";
            
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                User = new { Id = userId, Role = userRole },
                Cache = new { 
                    Appointments = _cache.TryGetValue("gateway_appointments_all", out _),
                    Staff = _cache.TryGetValue("gateway_staff_all", out _)
                }
            });
        }

        #endregion
    }
}
