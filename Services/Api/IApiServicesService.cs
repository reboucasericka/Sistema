using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sistema.Services.Api
{
    public interface IApiServicesService
    {
        Task<ApiResponse<IEnumerable<ServiceDto>>> GetAllAsync();
        Task<ApiResponse<ServiceDto>> GetByIdAsync(int id);
        Task<ApiResponse<ServiceDto>> CreateAsync(ServiceDto service);
        Task<ApiResponse<ServiceDto>> UpdateAsync(int id, ServiceDto service);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<ServiceDto>>> GetActiveAsync();
        Task<ApiResponse<IEnumerable<ServiceDto>>> GetByCategoryIdAsync(int categoryId);
    }
}
