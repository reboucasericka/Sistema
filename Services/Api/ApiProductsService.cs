using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;
using System.Collections.Generic;

namespace Sistema.Services.Api
{
    public class ApiProductsService : ApiClient, IApiProductsService
    {
        public ApiProductsService(HttpClient httpClient, ILogger<ApiProductsService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetAllAsync()
        {
            return await GetAsync<IEnumerable<ProductDto>>("products");
        }

        public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
        {
            return await GetAsync<ProductDto>($"products/{id}");
        }

        public async Task<ApiResponse<ProductDto>> CreateAsync(ProductDto product)
        {
            return await PostAsync<ProductDto>("products", product);
        }

        public async Task<ApiResponse<ProductDto>> UpdateAsync(int id, ProductDto product)
        {
            return await PutAsync<ProductDto>($"products/{id}", product);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"products/{id}");
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetByCategoryAsync(int categoryId)
        {
            return await GetAsync<IEnumerable<ProductDto>>($"products/category/{categoryId}");
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetLowStockAsync()
        {
            return await GetAsync<IEnumerable<ProductDto>>("products/low-stock");
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> SearchAsync(string searchTerm)
        {
            return await GetAsync<IEnumerable<ProductDto>>($"products/search?q={Uri.EscapeDataString(searchTerm)}");
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetActiveAsync()
        {
            return await GetAsync<IEnumerable<ProductDto>>("products/active");
        }

        public async Task<ApiResponse<IEnumerable<ProductDto>>> GetByCategoryIdAsync(int categoryId)
        {
            return await GetByCategoryAsync(categoryId);
        }
    }
}
