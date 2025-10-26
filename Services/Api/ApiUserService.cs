using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;

namespace Sistema.Services.Api
{
    public class ApiUserService : IApiUserService
    {
        private readonly IApiClientService _apiClient;
        private readonly ILogger<ApiUserService> _logger;

        public ApiUserService(IApiClientService apiClient, ILogger<ApiUserService> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<UserDto>>> GetUsersAsync()
        {
            _logger.LogInformation("Fetching all users from API");
            return await _apiClient.GetAsync<IEnumerable<UserDto>>("api/v1/staff");
        }

        public async Task<ApiResponse<UserDto>> GetUserAsync(string id)
        {
            _logger.LogInformation($"Fetching user {id} from API");
            return await _apiClient.GetAsync<UserDto>($"api/v1/staff/{id}");
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(UserDto user)
        {
            _logger.LogInformation($"Creating new user {user.Email}");
            return await _apiClient.PostAsync<UserDto>("api/v1/staff", user);
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UserDto user)
        {
            _logger.LogInformation($"Updating user {id}");
            return await _apiClient.PutAsync<UserDto>($"api/v1/staff/{id}", user);
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(string id)
        {
            _logger.LogInformation($"Deleting user {id}");
            return await _apiClient.DeleteAsync<bool>($"api/v1/staff/{id}");
        }

        public async Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetProfessionalsAsync()
        {
            _logger.LogInformation("Fetching all professionals from API");
            return await _apiClient.GetAsync<IEnumerable<ProfessionalDto>>("api/v1/staff");
        }

        public async Task<ApiResponse<ProfessionalDto>> GetProfessionalAsync(int id)
        {
            _logger.LogInformation($"Fetching professional {id} from API");
            return await _apiClient.GetAsync<ProfessionalDto>($"api/v1/staff/{id}");
        }
    }
}
