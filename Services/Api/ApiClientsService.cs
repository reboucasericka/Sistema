using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiClientsService : ApiClient, IApiClientsService
    {
        public ApiClientsService(HttpClient httpClient, ILogger<ApiClientsService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<ClientDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<ClientDto>>("clients");
        }

        public async Task<ApiResponse<ClientDto>> GetByIdAsync(int id)
        {
            return await GetAsync<ClientDto>($"clients/{id}");
        }

        public async Task<ApiResponse<ClientDto>> CreateAsync(ClientDto client)
        {
            return await PostAsync<ClientDto>("clients", client);
        }

        public async Task<ApiResponse<ClientDto>> UpdateAsync(int id, ClientDto client)
        {
            return await PutAsync<ClientDto>($"clients/{id}", client);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"clients/{id}");
        }

        public async Task<ApiResponse<IEnumerable<ClientDto>>> SearchAsync(string searchTerm)
        {
            return await GetAsync<IEnumerable<ClientDto>>($"clients/search?q={Uri.EscapeDataString(searchTerm)}");
        }
    }
}
