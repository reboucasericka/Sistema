using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;

namespace Sistema.Services.Api
{
    public interface IApiAppointmentService
    {
        Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAppointmentsAsync();
        Task<ApiResponse<AppointmentDto>> GetAppointmentAsync(int id);
        Task<ApiResponse<AppointmentDto>> CreateAppointmentAsync(AppointmentDto appointment);
        Task<ApiResponse<AppointmentDto>> UpdateAppointmentAsync(int id, AppointmentDto appointment);
        Task<ApiResponse<bool>> DeleteAppointmentAsync(int id);
        Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAppointmentsByCustomerAsync(int customerId);
        Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAppointmentsByProfessionalAsync(int professionalId);
    }
}
