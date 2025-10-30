using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Helpers;
using Sistema.Models.Account;
using Sistema.Services;
using Microsoft.Extensions.Logging;
using Sistema.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    [AllowAnonymous]
    public class PublicAccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IEmailService _emailService;
        private readonly ILogger<PublicAccountController> _logger;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public PublicAccountController(
            IUserHelper userHelper, 
            IEmailService emailService,
            ILogger<PublicAccountController> logger,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IConfiguration configuration)
        {
            _userHelper = userHelper;
            _emailService = emailService;
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        // =======================
        // LOGIN DE CLIENTE
        // =======================
        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            // 🔒 VERIFICAÇÃO SEGURA: Se já estiver autenticado E com role Customer
            if (User.Identity?.IsAuthenticated == true)
            {
                // Verificar se tem a role Customer antes de redirecionar
                if (User.IsInRole("Customer"))
                {
                    _logger.LogInformation("Usuário já autenticado com role Customer, redirecionando para painel");
                    return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
                }
                else
                {
                    // Se autenticado mas sem role Customer, fazer logout e permitir novo login
                    _logger.LogWarning("Usuário autenticado mas sem role Customer, fazendo logout");
                    await _signInManager.SignOutAsync();
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            // 🔒 VALIDAÇÃO DE SEGURANÇA
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Tentativa de login com ModelState inválido");
                return View(model);
            }

            try
            {
                // 🔍 BUSCA SEGURA DO USUÁRIO
                var user = await _userHelper.GetUserByEmailAsync(model.Username) 
                          ?? await _userHelper.GetUserByUsernameAsync(model.Username);

                if (user == null)
                {
                    _logger.LogWarning("Tentativa de login com usuário inexistente: {Username}", model.Username);
                    ModelState.AddModelError("", "E-mail ou senha incorretos.");
                    return View(model);
                }

                // 🔒 VERIFICAÇÃO DE CONTA ATIVA
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Tentativa de login com conta não confirmada: {Email}", user.Email);
                    TempData["InfoMessage"] = "Ative sua conta pelo e-mail antes de entrar.";
                    return RedirectToAction("ActivationPending");
                }

                // 🔐 AUTENTICAÇÃO SEGURA COM LOCKOUT
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: true); // 🔒 Ativa lockout em caso de falha

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("Conta bloqueada por tentativas excessivas: {Email}", user.Email);
                        ModelState.AddModelError("", "Sua conta foi temporariamente bloqueada. Tente novamente mais tarde.");
                    }
                    else
                    {
                        _logger.LogWarning("Falha na autenticação para {Email}: {Result}", user.Email, result);
                        ModelState.AddModelError("", "E-mail ou senha incorretos.");
                    }
                    return View(model);
                }

                // 🔍 VERIFICAÇÃO E ATRIBUIÇÃO DE ROLE
                var roles = await _userHelper.GetUserRolesAsync(user);
                if (!roles.Contains("Customer"))
                {
                    _logger.LogInformation("Adicionando role Customer para usuário {Email}", user.Email);
                    await _userHelper.AddUserToRoleAsync(user, "Customer");
                }

                // 🔄 ATUALIZAÇÃO SEGURA DO COOKIE DE AUTENTICAÇÃO
                await _signInManager.SignInAsync(user, model.RememberMe);

                _logger.LogInformation("Login bem-sucedido para {Email} com roles: {Roles}", 
                    user.Email, string.Join(", ", roles));

                // 🎯 REDIRECIONAMENTO SEGURO
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante processo de login para {Username}", model.Username);
                ModelState.AddModelError("", "Ocorreu um erro interno. Tente novamente.");
                return View(model);
            }
        }

        // =======================
        // REGISTRO DE CLIENTE
        // =======================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "PublicAppointment", new { area = "Public" });

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Verificar se o e-mail já existe
                var existingUser = await _userHelper.GetUserByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Este e-mail já está registrado.");
                    return View(model);
                }

                // Verificar se o username já existe
                var existingUsername = await _userHelper.GetUserByUsernameAsync(model.Username);
                if (existingUsername != null)
                {
                    ModelState.AddModelError("Username", "Este username já está em uso.");
                    return View(model);
                }

                // Log do novo registro
                _logger.LogInformation("Novo registro de cliente: {Email}", model.Email);

                // Criar novo usuário
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.Phone,
                    UserName = model.Username,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.Now
                };

                // Adicionar usuário
                var result = await _userHelper.AddUserAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }

                // Adicionar role de Customer
                await _userHelper.AddUserToRoleAsync(user, "Customer");

                // Gerar token de ativação
                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);

                // Montar link de ativação
                var activationLink = Url.Action("ActivateAccount", "PublicAccount", 
                    new { area = "Public", userId = user.Id, token }, Request.Scheme);

                // Enviar e-mail de ativação
                await _emailService.SendActivationEmailAsync(user.Email, user.FirstName, activationLink);

                // Log de registro
                _logger.LogInformation($"Novo cliente registrado: {user.Email}");

                // Armazenar e-mail no TempData para exibir na tela de pendência
                TempData["UserEmail"] = user.Email;

                return RedirectToAction("ActivationPending");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar cliente");
                ModelState.AddModelError("", "Ocorreu um erro ao criar sua conta. Tente novamente.");
                return View(model);
            }
        }

        // =======================
        // ATIVAÇÃO DE CONTA
        // =======================
        [HttpGet]
        public IActionResult ActivationPending()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ActivateAccount(string userId, string token)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Link de ativação inválido.";
                    return RedirectToAction("ActivationPending");
                }

                var user = await _userHelper.GetUserByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuário não encontrado.";
                    return RedirectToAction("ActivationPending");
                }

                if (user.EmailConfirmed)
                {
                    TempData["InfoMessage"] = "Sua conta já está ativada.";
                    return RedirectToAction("ActivateSuccess");
                }

                var result = await _userHelper.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Conta ativada com sucesso: {user.Email}");
                    
                    // Fazer login automático após ativação
                    await _signInManager.SignInAsync(user, false);
                    
                    // Armazenar dados do usuário para exibir na tela de sucesso
                    TempData["UserFirstName"] = user.FirstName;
                    
                    return RedirectToAction("ActivateSuccess");
                }
                else
                {
                    _logger.LogWarning($"Falha na ativação da conta: {user.Email}");
                    TempData["ErrorMessage"] = "Falha ao ativar a conta. O link pode ter expirado.";
                    return RedirectToAction("ActivationPending");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ativar conta");
                TempData["ErrorMessage"] = "Ocorreu um erro ao ativar sua conta.";
                return RedirectToAction("ActivationPending");
            }
        }

        [HttpGet]
        public IActionResult ActivateSuccess()
        {
            return View();
        }

        // =======================
        // GOOGLE CALENDAR OAUTH
        // =======================
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public IActionResult LinkGoogleCalendar()
        {
            var clientId = _configuration["GoogleCalendar:ClientId"];
            var redirectUri = _configuration["GoogleCalendar:RedirectUri"];
            var scope = "https://www.googleapis.com/auth/calendar.events";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            {
                TempData["ErrorMessage"] = "Configuração do Google Calendar ausente.";
                return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
            }

            var authUrl = $"https://accounts.google.com/o/oauth2/auth?client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={Uri.EscapeDataString(scope)}&access_type=offline&prompt=consent";
            return Redirect(authUrl);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback(string code, string? error = null)
        {
            if (!string.IsNullOrEmpty(error) || string.IsNullOrEmpty(code))
            {
                TempData["ErrorMessage"] = "Não foi possível vincular o Google Calendar.";
                return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
            }

            try
            {
                var clientId = _configuration["GoogleCalendar:ClientId"];
                var clientSecret = _configuration["GoogleCalendar:ClientSecret"];
                var redirectUri = _configuration["GoogleCalendar:RedirectUri"];

                using var http = new HttpClient();
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = clientId!,
                    ["client_secret"] = clientSecret!,
                    ["redirect_uri"] = redirectUri!,
                    ["grant_type"] = "authorization_code"
                });

                var resp = await http.PostAsync("https://oauth2.googleapis.com/token", content);
                var json = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro ao trocar código por token Google: {Status} {Body}", resp.StatusCode, json);
                    TempData["ErrorMessage"] = "Falha ao vincular Google Calendar.";
                    return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
                }

                // Armazena o access token na sessão (poderíamos persistir por usuário futuramente)
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var accessToken = doc.RootElement.GetProperty("access_token").GetString();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    HttpContext.Session.SetString("GoogleAccessToken", accessToken);
                }

                TempData["SuccessMessage"] = "Google Calendar vinculado com sucesso!";
                return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao processar GoogleCallback");
                TempData["ErrorMessage"] = "Erro ao processar retorno do Google.";
                return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
            }
        }

        // =======================
        // LOGIN SOCIAL (GOOGLE / FACEBOOK)
        // =======================
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // 'provider' deve ser "Google" ou "Facebook"
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "PublicAccount", new { area = "Public", ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Erro do provedor externo: {remoteError}");
                return Redirect("/Public/PublicAccount/Login");
            }

            // Pega as informações do login externo
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect("/Public/PublicAccount/Login");
            }

            // Tenta autenticar com login externo
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // Login externo OK. Redireciona para área pública.
                return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
            }
            else
            {
                // Se o utilizador ainda não existir, pode criar nova conta ou associar ao login externo
                var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (email != null)
                {
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        // Cria novo utilizador se não existir
                        user = new User
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true // Confirma automaticamente emails sociais
                        };
                        await _userManager.CreateAsync(user);
                        await _userHelper.AddUserToRoleAsync(user, "Customer");
                    }
                    // Associa login externo ao utilizador criado
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "PublicClientPanel", new { area = "Public" });
                }

                // Não conseguiu autenticar
                ModelState.AddModelError(string.Empty, "Não foi possível autenticar com Google ou Facebook");
                return Redirect("/Public/PublicAccount/Login");
            }
        }
    }
}
