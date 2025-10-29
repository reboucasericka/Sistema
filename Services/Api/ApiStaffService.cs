using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiStaffService : ApiClient, IApiStaffService
    {
        public ApiStaffService(HttpClient httpClient, ILogger<ApiStaffService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<ProfessionalDto>>("staff");
        }

        public async Task<ApiResponse<ProfessionalDto>> GetByIdAsync(int id)
        {
            return await GetAsync<ProfessionalDto>($"staff/{id}");
        }

        public async Task<ApiResponse<ProfessionalDto>> CreateAsync(ProfessionalDto professional)
        {
            return await PostAsync<ProfessionalDto>("staff", professional);
        }

        public async Task<ApiResponse<ProfessionalDto>> UpdateAsync(int id, ProfessionalDto professional)
        {
            return await PutAsync<ProfessionalDto>($"staff/{id}", professional);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"staff/{id}");
        }

        public async Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetActiveAsync()
        {
            return await GetAsync<IEnumerable<ProfessionalDto>>("staff/active");
        }

        public async Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetBySpecialtyAsync(string specialty)
        {
            return await GetAsync<IEnumerable<ProfessionalDto>>($"staff/specialty/{Uri.EscapeDataString(specialty)}");
        }

        public async Task<ApiResponse<IEnumerable<ProfessionalDto>>> SearchAsync(string searchTerm)
        {
            return await GetAsync<IEnumerable<ProfessionalDto>>($"staff/search?q={Uri.EscapeDataString(searchTerm)}");
        }
    }
}
