using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;

namespace Sistema.Services.Api
{
    public interface IApiClientsService
    {
        Task<ApiResponse<IEnumerable<ClientDto>>> GetAllAsync();
        Task<ApiResponse<ClientDto>> GetByIdAsync(int id);
        Task<ApiResponse<ClientDto>> CreateAsync(ClientDto client);
        Task<ApiResponse<ClientDto>> UpdateAsync(int id, ClientDto client);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<ClientDto>>> SearchAsync(string searchTerm);
    }
}
