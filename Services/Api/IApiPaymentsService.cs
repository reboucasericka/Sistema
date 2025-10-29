using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sistema.Services.Api
{
    public interface IApiPaymentsService
    {
        Task<ApiResponse<IEnumerable<PaymentDto>>> GetAllAsync();
        Task<ApiResponse<PaymentDto>> GetByIdAsync(int id);
        Task<ApiResponse<PaymentDto>> CreateAsync(PaymentDto payment);
        Task<ApiResponse<PaymentDto>> UpdateAsync(int id, PaymentDto payment);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<PaymentDto>>> GetByClientIdAsync(int clientId);
        Task<ApiResponse<IEnumerable<PaymentDto>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
