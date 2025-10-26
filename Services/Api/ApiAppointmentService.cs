using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;

namespace Sistema.Services.Api
{
    public class ApiAppointmentService : IApiAppointmentService
    {
        private readonly IApiClientService _apiClient;
        private readonly ILogger<ApiAppointmentService> _logger;

        public ApiAppointmentService(IApiClientService apiClient, ILogger<ApiAppointmentService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAppointmentsAsync()
        {
            _logger.LogInformation("Fetching all appointments from API");
            return await _apiClient.GetAsync<IEnumerable<AppointmentDto>>("api/v1/appointments");
        }

        public async Task<ApiResponse<AppointmentDto>> GetAppointmentAsync(int id)
        {
            _logger.LogInformation($"Fetching appointment {id} from API");
            return await _apiClient.GetAsync<AppointmentDto>($"api/v1/appointments/{id}");
        }

        public async Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(AppointmentDto appointment)
        {
            _logger.LogInformation($"Creating new appointment for client {appointment.ClientId}");
            return await _apiClient.PostAsync<AppointmentDto>("api/v1/appointments", appointment);
        }

        public async Task<ApiResponse<AppointmentDto>> UpdateAppointmentAsync(int id, AppointmentDto appointment)
        {
            _logger.LogInformation($"Updating appointment {id}");
            return await _apiClient.PutAsync<AppointmentDto>($"api/v1/appointments/{id}", appointment);
        }

        public async Task<ApiResponse<bool>> DeleteAppointmentAsync(int id)
        {
            _logger.LogInformation($"Deleting appointment {id}");
            return await _apiClient.DeleteAsync<bool>($"api/v1/appointments/{id}");
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAppointmentsByCustomerAsync(int customerId)
        {
            _logger.LogInformation($"Fetching appointments for customer {customerId}");
            return await _apiClient.GetAsync<IEnumerable<AppointmentDto>>($"api/v1/appointments/customer/{customerId}");
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAppointmentsByProfessionalAsync(int professionalId)
        {
            _logger.LogInformation($"Fetching appointments for professional {professionalId}");
            return await _apiClient.GetAsync<IEnumerable<AppointmentDto>>($"api/v1/appointments/professional/{professionalId}");
        }
    }
}
