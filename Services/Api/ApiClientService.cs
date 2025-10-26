using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Sistema.Services.Api
{
    public class ApiClientService : IApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiClientService> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public ApiClientService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ApiClientService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["SistemaApi:BaseUrl"] ?? "https://localhost:7001";
            _apiKey = _configuration["SistemaApi:ApiKey"] ?? "";
            
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("SistemaApi:Timeout", 30));
            
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            }
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            return await SendRequestAsync<T>(HttpMethod.Get, endpoint, null);
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, string token)
        {
            return await SendRequestAsync<T>(HttpMethod.Get, endpoint, null, token);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            return await SendRequestAsync<T>(HttpMethod.Post, endpoint, data);
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data, string token)
        {
            return await SendRequestAsync<T>(HttpMethod.Post, endpoint, data, token);
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            return await SendRequestAsync<T>(HttpMethod.Put, endpoint, data);
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            return await SendRequestAsync<T>(HttpMethod.Delete, endpoint, null);
        }

        private async Task<ApiResponse<T>> SendRequestAsync<T>(HttpMethod method, string endpoint, object? data, string? token = null)
        {
            try
            {
                var request = new HttpRequestMessage(method, endpoint);
                
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                if (data != null)
                {
                    var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                _logger.LogInformation($"Sending {method} request to {endpoint}");
                
                var response = await _httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Received response: {response.StatusCode} - {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    return result ?? ApiResponse<T>.Fail("Failed to deserialize response");
                }
                else
                {
                    _logger.LogError($"API request failed: {response.StatusCode} - {responseContent}");
                    return ApiResponse<T>.Fail($"API request failed: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP request exception for {method} {endpoint}");
                return ApiResponse<T>.Fail($"Network error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"Request timeout for {method} {endpoint}");
                return ApiResponse<T>.Fail("Request timeout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error for {method} {endpoint}");
                return ApiResponse<T>.Fail($"Unexpected error: {ex.Message}");
            }
        }
    }
}
