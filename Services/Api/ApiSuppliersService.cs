using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;

namespace Sistema.Services.Api
{
    public interface IApiSuppliersService
    {
        Task<ApiResponse<List<SupplierDto>>> GetAllAsync();
        Task<ApiResponse<SupplierDto>> GetByIdAsync(int id);
        Task<ApiResponse<SupplierDto>> CreateAsync(SupplierDto supplier);
        Task<ApiResponse<SupplierDto>> UpdateAsync(int id, SupplierDto supplier);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }

    public class ApiSuppliersService : ApiClient, IApiSuppliersService
    {
        public ApiSuppliersService(HttpClient httpClient, ILogger<ApiSuppliersService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<List<SupplierDto>>> GetAllAsync()
        {
            return await GetAsync<List<SupplierDto>>("suppliers");
        }

        public async Task<ApiResponse<SupplierDto>> GetByIdAsync(int id)
        {
            return await GetAsync<SupplierDto>($"suppliers/{id}");
        }

        public async Task<ApiResponse<SupplierDto>> CreateAsync(SupplierDto supplier)
        {
            return await PostAsync<SupplierDto>("suppliers", supplier);
        }

        public async Task<ApiResponse<SupplierDto>> UpdateAsync(int id, SupplierDto supplier)
        {
            return await PutAsync<SupplierDto>($"suppliers/{id}", supplier);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"suppliers/{id}");
        }
    }
}