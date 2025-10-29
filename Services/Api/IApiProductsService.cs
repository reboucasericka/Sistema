using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sistema.Services.Api
{
    public interface IApiProductsService
    {
        Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync();
        Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateAsync(ProductDto product);
        Task<ApiResponse<ProductDto>> UpdateAsync(int id, ProductDto product);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetActiveAsync();
        Task<ApiResponse<IEnumerable<ProductDto>>> GetByCategoryIdAsync(int categoryId);
        Task<ApiResponse<IEnumerable<ProductDto>>> GetLowStockAsync();
    }
}
