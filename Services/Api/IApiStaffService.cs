using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sistema.Services.Api
{
    public interface IApiStaffService
    {
        Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetAllAsync();
        Task<ApiResponse<ProfessionalDto>> GetByIdAsync(int id);
        Task<ApiResponse<ProfessionalDto>> CreateAsync(ProfessionalDto professional);
        Task<ApiResponse<ProfessionalDto>> UpdateAsync(int id, ProfessionalDto professional);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetActiveAsync();
        Task<ApiResponse<IEnumerable<ProfessionalDto>>> GetBySpecialtyAsync(string specialty);
    }
}
