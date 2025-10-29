using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public interface IApiNotificationsService
    {
        Task<ApiResponse<IEnumerable<NotificationDto>>> GetAllAsync();
        Task<ApiResponse<NotificationDto>> GetByIdAsync(int id);
        Task<ApiResponse<NotificationDto>> CreateAsync(NotificationDto notification);
        Task<ApiResponse<NotificationDto>> UpdateAsync(int id, NotificationDto notification);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> MarkAsReadAsync(int id);
    }
}
