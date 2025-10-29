using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sistema.Services.Api
{
    public interface IApiAppointmentsService
    {
        Task<ApiResponse<IEnumerable<AppointmentDto>>> GetAllAsync();
        Task<ApiResponse<AppointmentDto>> GetByIdAsync(int id);
        Task<ApiResponse<AppointmentDto>> CreateAsync(AppointmentDto appointment);
        Task<ApiResponse<AppointmentDto>> UpdateAsync(int id, AppointmentDto appointment);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByClientIdAsync(int clientId);
        Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByProfessionalIdAsync(int professionalId);
        Task<ApiResponse<IEnumerable<AppointmentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
