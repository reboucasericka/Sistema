using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using Sistema.Services.Auth;

namespace Sistema.Services.Api
{
    public abstract class ApiClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly ApiAuthService _auth;

        public ApiClient(HttpClient httpClient, ILogger logger, ApiAuthService auth)
        {
            _httpClient = httpClient;
            _logger = logger;
            _auth = auth;
        }

        protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation($"GET request to {endpoint}");
                var client = await _auth.WithAuthAsync();
                var response = await client.GetAsync($"https://localhost:5025/api/v1/{endpoint}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                    return result ?? ApiResponse<T>.Fail("Failed to deserialize response");
                }
                else
                {
                    _logger.LogError($"API request failed: {response.StatusCode}");
                    return ApiResponse<T>.Fail($"API request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GET request to {endpoint}");
                return ApiResponse<T>.Fail($"Network error: {ex.Message}");
            }
        }

        protected async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                _logger.LogInformation($"POST request to {endpoint}");
                var client = await _auth.WithAuthAsync();
                var response = await client.PostAsJsonAsync($"https://localhost:5025/api/v1/{endpoint}", data);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                    return result ?? ApiResponse<T>.Fail("Failed to deserialize response");
                }
                else
                {
                    _logger.LogError($"API request failed: {response.StatusCode}");
                    return ApiResponse<T>.Fail($"API request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in POST request to {endpoint}");
                return ApiResponse<T>.Fail($"Network error: {ex.Message}");
            }
        }

        protected async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                _logger.LogInformation($"PUT request to {endpoint}");
                var client = await _auth.WithAuthAsync();
                var response = await client.PutAsJsonAsync($"https://localhost:5025/api/v1/{endpoint}", data);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
                    return result ?? ApiResponse<T>.Fail("Failed to deserialize response");
                }
                else
                {
                    _logger.LogError($"API request failed: {response.StatusCode}");
                    return ApiResponse<T>.Fail($"API request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in PUT request to {endpoint}");
                return ApiResponse<T>.Fail($"Network error: {ex.Message}");
            }
        }

        protected async Task<ApiResponse<bool>> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation($"DELETE request to {endpoint}");
                var client = await _auth.WithAuthAsync();
                var response = await client.DeleteAsync($"https://localhost:5025/api/v1/{endpoint}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                    return result ?? ApiResponse<bool>.Fail("Failed to deserialize response");
                }
                else
                {
                    _logger.LogError($"API request failed: {response.StatusCode}");
                    return ApiResponse<bool>.Fail($"API request failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in DELETE request to {endpoint}");
                return ApiResponse<bool>.Fail($"Network error: {ex.Message}");
            }
        }
    }
}
