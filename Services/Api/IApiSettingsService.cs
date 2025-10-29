using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public interface IApiSettingsService
    {
        Task<ApiResponse<IEnumerable<SettingDto>>> GetAllAsync();
        Task<ApiResponse<SettingDto>> GetByIdAsync(int id);
        Task<ApiResponse<SettingDto>> CreateAsync(SettingDto setting);
        Task<ApiResponse<SettingDto>> UpdateAsync(int id, SettingDto setting);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
