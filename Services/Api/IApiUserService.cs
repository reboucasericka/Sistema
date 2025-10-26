using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;

namespace Sistema.Services.Api
{
    public interface IApiUserService
    {
        Task<ApiResponse<IEnumerable<UserDto>>> GetUsersAsync();
        Task<ApiResponse<UserDto>> GetUserAsync(string id);
        Task<ApiResponse<UserDto>> CreateUserAsync(UserDto user);
        Task<ApiResponse<UserDto>> UpdateUserAsync(string id, UserDto user);
        Task<ApiResponse<bool>> DeleteUserAsync(string id);
        Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetProfessionalsAsync();
        Task<ApiResponse<ProfessionalDto>> GetProfessionalAsync(int id);
    }
}
