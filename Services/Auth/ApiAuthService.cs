using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Sistema.Services.Auth
{
    public class ApiAuthService
    {
        private readonly HttpClient _http;
        private string? _token;

        public ApiAuthService(HttpClient http)
        {
            _http = http;
            _http.BaseAddress = new Uri("https://localhost:5025/api/auth/");
        }

        // Faz login e guarda o token JWT
        public async Task<string?> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_token))
                return _token;

            var body = new { email = "admin@admin.com", password = "admin" };
            var response = await _http.PostAsJsonAsync("login", body);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
            _token = json?.token;
            return _token;
        }

        // Configura o HttpClient com o Bearer Token
        public async Task<HttpClient> WithAuthAsync()
        {
            var token = await GetTokenAsync();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return _http;
        }

        private class LoginResponse
        {
            public string? token { get; set; }
        }
    }
}
