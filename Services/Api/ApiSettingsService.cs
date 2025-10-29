using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiSettingsService : ApiClient, IApiSettingsService
    {
        public ApiSettingsService(HttpClient httpClient, ILogger<ApiSettingsService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<SettingDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<SettingDto>>("settings");
        }

        public async Task<ApiResponse<SettingDto>> GetByIdAsync(int id)
        {
            return await GetAsync<SettingDto>($"settings/{id}");
        }

        public async Task<ApiResponse<SettingDto>> CreateAsync(SettingDto setting)
        {
            return await PostAsync<SettingDto>("settings", setting);
        }

        public async Task<ApiResponse<SettingDto>> UpdateAsync(int id, SettingDto setting)
        {
            return await PutAsync<SettingDto>($"settings/{id}", setting);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"settings/{id}");
        }
    }
}
