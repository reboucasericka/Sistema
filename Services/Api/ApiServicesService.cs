using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiServicesService : ApiClient
    {
        public ApiServicesService(HttpClient httpClient, ILogger<ApiServicesService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<ServiceDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<ServiceDto>>("services");
        }

        public async Task<ApiResponse<ServiceDto>> GetByIdAsync(int id)
        {
            return await GetAsync<ServiceDto>($"services/{id}");
        }

        public async Task<ApiResponse<ServiceDto>> CreateAsync(ServiceDto service)
        {
            return await PostAsync<ServiceDto>("services", service);
        }

        public async Task<ApiResponse<ServiceDto>> UpdateAsync(int id, ServiceDto service)
        {
            return await PutAsync<ServiceDto>($"services/{id}", service);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"services/{id}");
        }

        public async Task<ApiResponse<IEnumerable<ServiceDto>>> GetByCategoryAsync(int categoryId)
        {
            return await GetAsync<IEnumerable<ServiceDto>>($"services/category/{categoryId}");
        }

        public async Task<ApiResponse<IEnumerable<ServiceDto>>> GetActiveAsync()
        {
            return await GetAsync<IEnumerable<ServiceDto>>("services/active");
        }

        public async Task<ApiResponse<IEnumerable<ServiceDto>>> SearchAsync(string searchTerm)
        {
            return await GetAsync<IEnumerable<ServiceDto>>($"services/search?q={Uri.EscapeDataString(searchTerm)}");
        }
    }
}
