using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;

namespace Sistema.Services.Api
{
    public interface IApiClientService
    {
        Task<ApiResponse<T>> GetAsync<T>(string endpoint);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data);
        Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
        Task<ApiResponse<T>> DeleteAsync<T>(string endpoint);
        Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string token);
        Task<ApiResponse<T>> GetAsync<T>(string endpoint, string token);
    }
}
