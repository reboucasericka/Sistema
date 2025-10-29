using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace Sistema.Services.Auth
{
    public class ApiAuthService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private string? _token;

        public ApiAuthService(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
            
            // BaseAddress será configurado automaticamente pelo DI no Program.cs
            // Aqui apenas definimos o endpoint específico de auth
            var apiSettings = _configuration.GetSection("ApiSettings");
            var baseUrl = apiSettings["BaseUrl"] ?? "https://localhost:7001/api";
            _http.BaseAddress = new Uri($"{baseUrl}/auth/");
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

        // Método de login com credenciais específicas
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            try
            {
                var body = new { email = username, password = password };
                var response = await _http.PostAsJsonAsync("login", body);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResult { Success = false, Message = "Credenciais inválidas" };
                }

                var json = await response.Content.ReadFromJsonAsync<LoginResponse>();
                _token = json?.token;

                if (string.IsNullOrEmpty(_token))
                {
                    return new LoginResult { Success = false, Message = "Token não recebido" };
                }

                // Implementar verificação de roles via API
                var userRoles = new List<string>();
                
                try
                {
                    // Verificar se é admin baseado no email
                    if (username?.Contains("admin") == true || 
                        username?.Contains("@admin.") == true)
                    {
                        userRoles.Add("Admin");
                    }
                    else
                    {
                        userRoles.Add("User");
                    }
                }
                catch
                {
                    userRoles.Add("User"); // Role padrão em caso de erro
                }
                
                return new LoginResult 
                { 
                    Success = true, 
                    Token = _token,
                    Data = new UserData { Roles = userRoles.ToArray() }
                };
            }
            catch (Exception ex)
            {
                return new LoginResult { Success = false, Message = ex.Message };
            }
        }

        public class LoginResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public string? Token { get; set; }
            public UserData? Data { get; set; }
        }

        public class UserData
        {
            public string[]? Roles { get; set; }
        }

        private class LoginResponse
        {
            public string? token { get; set; }
        }
    }
}
