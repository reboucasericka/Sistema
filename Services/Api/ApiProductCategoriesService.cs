using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;

namespace Sistema.Services.Api
{
    public interface IApiProductCategoriesService
    {
        Task<ApiResponse<List<ProductCategoryDto>>> GetAllAsync();
        Task<ApiResponse<ProductCategoryDto>> GetByIdAsync(int id);
        Task<ApiResponse<ProductCategoryDto>> CreateAsync(ProductCategoryDto category);
        Task<ApiResponse<ProductCategoryDto>> UpdateAsync(int id, ProductCategoryDto category);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }

    public class ApiProductCategoriesService : ApiClient, IApiProductCategoriesService
    {
        public ApiProductCategoriesService(HttpClient httpClient, ILogger<ApiProductCategoriesService> logger, ApiAuthService auth) 
            : base(httpClient, logger, auth)
        {
        }

        public async Task<ApiResponse<List<ProductCategoryDto>>> GetAllAsync()
        {
            return await GetAsync<List<ProductCategoryDto>>("productcategories");
        }

        public async Task<ApiResponse<ProductCategoryDto>> GetByIdAsync(int id)
        {
            return await GetAsync<ProductCategoryDto>($"productcategories/{id}");
        }

        public async Task<ApiResponse<ProductCategoryDto>> CreateAsync(ProductCategoryDto category)
        {
            return await PostAsync<ProductCategoryDto>("productcategories", category);
        }

        public async Task<ApiResponse<ProductCategoryDto>> UpdateAsync(int id, ProductCategoryDto category)
        {
            return await PutAsync<ProductCategoryDto>($"productcategories/{id}", category);
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            return await DeleteAsync($"productcategories/{id}");
        }
    }
}