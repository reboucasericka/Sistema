using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiNotificationsService : ApiClient, IApiNotificationsService
    {
        public ApiNotificationsService(HttpClient httpClient, ILogger<ApiNotificationsService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<NotificationDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<NotificationDto>>("notifications");
        }

        public async Task<ApiResponse<NotificationDto>> GetByIdAsync(int id)
        {
            return await GetAsync<NotificationDto>($"notifications/{id}");
        }

        public async Task<ApiResponse<NotificationDto>> CreateAsync(NotificationDto notification)
        {
            return await PostAsync<NotificationDto>("notifications", notification);
        }

        public async Task<ApiResponse<NotificationDto>> UpdateAsync(int id, NotificationDto notification)
        {
            return await PutAsync<NotificationDto>($"notifications/{id}", notification);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"notifications/{id}");
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(int id)
        {
            return await PutAsync<bool>($"notifications/{id}/mark-read", null);
        }
    }
}
