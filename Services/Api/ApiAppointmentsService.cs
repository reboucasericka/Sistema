using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiAppointmentsService : ApiClient, IApiAppointmentsService
    {
        public ApiAppointmentsService(HttpClient httpClient, ILogger<ApiAppointmentsService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<AppointmentDto>>("appointments");
        }

        public async Task<ApiResponse<AppointmentDto>> GetByIdAsync(int id)
        {
            return await GetAsync<AppointmentDto>($"appointments/{id}");
        }

        public async Task<ApiResponse<AppointmentDto>> CreateAsync(AppointmentDto appointment)
        {
            return await PostAsync<AppointmentDto>("appointments", appointment);
        }

        public async Task<ApiResponse<AppointmentDto>> UpdateAsync(int id, AppointmentDto appointment)
        {
            return await PutAsync<AppointmentDto>($"appointments/{id}", appointment);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"appointments/{id}");
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByDateAsync(DateTime date)
        {
            return await GetAsync<IEnumerable<AppointmentDto>>($"appointments/date/{date:yyyy-MM-dd}");
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByProfessionalAsync(int professionalId)
        {
            return await GetAsync<IEnumerable<AppointmentDto>>($"appointments/professional/{professionalId}");
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByCustomerAsync(int customerId)
        {
            return await GetAsync<IEnumerable<AppointmentDto>>($"appointments/customer/{customerId}");
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAvailableTimesAsync(int professionalId, DateTime date)
        {
            return await GetAsync<IEnumerable<AppointmentDto>>($"appointments/available/{professionalId}/{date:yyyy-MM-dd}");
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByClientIdAsync(int clientId)
        {
            return await GetByCustomerAsync(clientId);
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByProfessionalIdAsync(int professionalId)
        {
            return await GetByProfessionalAsync(professionalId);
        }

        public async Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await GetAsync<IEnumerable<AppointmentDto>>($"appointments/range/{startDate:yyyy-MM-dd}/{endDate:yyyy-MM-dd}");
        }
    }
}
