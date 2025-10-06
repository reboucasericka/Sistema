using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Sistema.Data.Entities;
using Sistema.Data;
using Sistema.Helpers;
using Sistema.Models.Account;
using Sistema.Models.Admin;
using Sistema.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;

namespace Sistema.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IEmailService _emailService;
        private readonly SistemaDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        

        public AccountController(IUserHelper userHelper, IEmailService emailService, SistemaDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userHelper = userHelper;
            _emailService = emailService;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            
        }

        // Helper method to get user by email or username
        private async Task<User?> GetCurrentUserAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return null;

            var identityName = User.Identity.Name;
            if (string.IsNullOrEmpty(identityName))
                return null;

            // Try email first, then username
            var user = await _userHelper.GetUserByEmailAsync(identityName);
            if (user == null)
            {
                user = await _userHelper.GetUserByUsernameAsync(identityName);
            }

            return user;
        }

        // Helper method to log user access
        private async Task LogAccess(User user, string action)
        {
            try
            {
                // Get user role
                var roles = await _userHelper.GetUserRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Unknown";

                // Get IP address
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                // Create access log entry
                var accessLog = new AccessLog
                {
                    UserId = user.Id,
                    Action = action,
                    Role = role,
                    Email = user.Email,
                    DateTime = DateTime.Now,
                    IpAddress = ipAddress
                };

                // Add to context and save
                _context.AccessLogs.Add(accessLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't break the main flow
                Console.WriteLine($"Erro ao registrar log de acesso: {ex.Message}");
            }
        }
        // =======================
        // LOGIN CLIENTE
        // =======================
        [HttpGet]
        public async Task<IActionResult> Login(string refresh = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Redirect authenticated users to appropriate area based on role
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    var roles = await _userHelper.GetUserRolesAsync(user);
                    var role = roles.FirstOrDefault() ?? "Unknown";
                    Console.WriteLine($"🔄 Usuário já autenticado: {user.Email} (Role: {role})");
                    
                    if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
                    {
                        Console.WriteLine($"🔄 Redirecionando Admin autenticado para: /Admin/Admin/Index");
                        return RedirectToAction("Index", "Admin", new { area = "Admin" });
                    }
                    else if (await _userHelper.IsUserInRoleAsync(user, "Customer"))
                    {
                        Console.WriteLine($"🔄 Redirecionando Customer autenticado para: /Public/Appointment/Index");
                        return RedirectToAction("Index", "Appointment", new { area = "Public" });
                    }
                }
                Console.WriteLine($"🔄 Redirecionando para área padrão: /Home/Index");
                return RedirectToAction("Index", "Home");
            }
            
            // Se veio do logout, adicionar headers para limpar cache
            if (!string.IsNullOrEmpty(refresh))
            {
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
            }
            
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Log da tentativa de login
                Console.WriteLine($"🔍 Tentativa de login: {model.Username}");
                
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    // Resolver usuário após login bem-sucedido
                    var user = await _userHelper.GetUserByEmailAsync(model.Username)
                               ?? await _userHelper.GetUserByUsernameAsync(model.Username);

                    if (user != null)
                    {
                        // Log successful login
                        await LogAccess(user, "Login");
                        
                        // Log do usuário e role
                        var roles = await _userHelper.GetUserRolesAsync(user);
                        var role = roles.FirstOrDefault() ?? "Unknown";
                        Console.WriteLine($"✅ Login bem-sucedido para: {user.Email} (Role: {role})");

                        // If there's a valid returnUrl, redirect there
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            Console.WriteLine($"🔄 Redirecionando para returnUrl: {returnUrl}");
                            return Redirect(returnUrl);
                        }

                        // Redirecionamento baseado em role
                        if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
                        {
                            Console.WriteLine($"🔄 Redirecionando Admin para: /Admin/Admin/Index");
                            return RedirectToAction("Index", "Admin", new { area = "Admin" });
                        }
                        else if (await _userHelper.IsUserInRoleAsync(user, "Customer"))
                        {
                            Console.WriteLine($"🔄 Redirecionando Customer para: /Public/Appointment/Index");
                            return RedirectToAction("Index", "Appointment", new { area = "Public" });
                        }
                        else
                        {
                            Console.WriteLine($"🔄 Redirecionando para área padrão: /Public/Appointment/Index");
                            return RedirectToAction("Index", "Appointment", new { area = "Public" });
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Login falhou para: {model.Username}");
                }
            }

            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return View(model);
        }

        // =======================
        // LOGIN VIA FACEBOOK E GOOGLE
        // =======================

        [HttpGet]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // 'provider' deve ser "Google" ou "Facebook"
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Erro do provedor externo: {remoteError}");
                return RedirectToAction("Login");
            }

            // Pega as informações do login externo
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            // Tenta autenticar com login externo
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                // Login externo OK. Redireciona.
                return LocalRedirect(returnUrl ?? "/");
            }
            else
            {
                // Se o utilizador ainda não existir, pode criar nova conta ou associar ao login externo
                var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);

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
                    return LocalRedirect(returnUrl ?? "/");
                }

                // Não conseguiu autenticar
                ModelState.AddModelError(string.Empty, "Não foi possível autenticar com Google ou Facebook");
                return RedirectToAction("Login");
            }
        }




        // =======================
        // LOGOUT
        // =======================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            try
            {
                Console.WriteLine("🔓 Iniciando logout...");
                
                // Get current user before logout to log the action
                var currentUser = await GetCurrentUserAsync();
                if (currentUser != null)
                {
                    // Log logout action
                    await LogAccess(currentUser, "Logout");
                }
                
                // Fazer logout do Identity
                await _userHelper.LogoutAsync();
                
                // Clear all authentication cookies
                foreach (var cookie in Request.Cookies.Keys)
                {
                    if (cookie.StartsWith(".AspNetCore."))
                    {
                        Response.Cookies.Delete(cookie);
                    }
                }
                
                // Adicionar headers para prevenir cache
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                
                Console.WriteLine("✅ Logout realizado com sucesso");
                
                // Redirect to login with parameter to force refresh
                return RedirectToAction("Login", "Account", new { refresh = DateTime.Now.Ticks });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no logout: {ex.Message}");
                return RedirectToAction("Login", "Account", new { refresh = DateTime.Now.Ticks });
            }
        }

        // =======================
        // REGISTRO CLIENTE
        // =======================
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterNewUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    user = new User
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        UserName = model.Username,
                        PhoneNumber = model.Phone
                    };

                    var result = await _userHelper.AddUserAsync(user, model.Password);
                    if (result != IdentityResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "Não foi possível criar o usuário. Tente novamente.");
                        return View(model);
                    }

                    // Role Customer
                    await _userHelper.AddUserToRoleAsync(user, "Customer");

                    // Send activation email
                    var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                    var activationLink = Url.Action("ActivateAccount", "Account", new { userId = user.Id, token }, Request.Scheme);

                    await _emailService.SendActivationEmailAsync(user.Email, user.FirstName, activationLink);

                    TempData["ShowActivationPopup"] = true;
                    TempData["UserEmail"] = user.Email;
                    TempData["UserName"] = user.FirstName;

                    // Redireciona para Login
                    return RedirectToAction("Login", "Account");
                }

                ModelState.AddModelError(string.Empty, "Já existe um usuário com este e-mail.");
            }

            return View(model);
        }



        // =======================
        // ATIVAR CONTA
        // =======================
        public async Task<IActionResult> ActivateAccount(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Link de ativação inválido.";
                return RedirectToAction("Login");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return RedirectToAction("Login");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Log successful account activation
                await LogAccess(user, "ActivateAccount");
                
                TempData["SuccessMessage"] = "Conta ativada com sucesso! Agora você pode entrar.";
                
                // Redirect based on user role
                if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                else if (await _userHelper.IsUserInRoleAsync(user, "Customer"))
                    return RedirectToAction("Index", "Appointment", new { area = "Public" });
                else
                    return RedirectToAction("Index", "Appointment", new { area = "Public" }); // Default to Customer area
            }

            TempData["ErrorMessage"] = "Erro ao ativar a conta. Link pode ter expirado.";
            return RedirectToAction("Login");
        }

        // =======================
        // DIAGNÓSTICO ADMIN
        // =======================
        [HttpGet]
        public async Task<IActionResult> DiagnoseAdmin()
        {
            Console.WriteLine("🔍 DIAGNÓSTICO ADMIN - Verificando usuário admin...");
            
            // Verificar se existe usuário admin
            var adminUsers = await _userHelper.GetUsersInRoleAsync("Admin");
            Console.WriteLine($"👥 Usuários Admin encontrados: {adminUsers.Count}");
            
            foreach (var admin in adminUsers)
            {
                Console.WriteLine($"   - ID: {admin.Id}");
                Console.WriteLine($"   - Email: {admin.Email}");
                Console.WriteLine($"   - UserName: {admin.UserName}");
                Console.WriteLine($"   - EmailConfirmed: {admin.EmailConfirmed}");
                Console.WriteLine($"   - Active: {admin.Active}");
            }
            
            // Verificar usuário específico por email
            var adminByEmail = await _userHelper.GetUserByEmailAsync("admin@admin.com");
            if (adminByEmail != null)
            {
                Console.WriteLine($"✅ Usuário admin@admin.com encontrado:");
                Console.WriteLine($"   - ID: {adminByEmail.Id}");
                Console.WriteLine($"   - UserName: {adminByEmail.UserName}");
                Console.WriteLine($"   - EmailConfirmed: {adminByEmail.EmailConfirmed}");
                
                var isAdmin = await _userHelper.IsUserInRoleAsync(adminByEmail, "Admin");
                Console.WriteLine($"   - IsAdmin: {isAdmin}");
            }
            else
            {
                Console.WriteLine("❌ Usuário admin@admin.com NÃO encontrado!");
            }
            
            // Verificar usuário por username
            var adminByUsername = await _userHelper.GetUserByUsernameAsync("admin");
            if (adminByUsername != null)
            {
                Console.WriteLine($"✅ Usuário 'admin' encontrado:");
                Console.WriteLine($"   - ID: {adminByUsername.Id}");
                Console.WriteLine($"   - Email: {adminByUsername.Email}");
                Console.WriteLine($"   - EmailConfirmed: {adminByUsername.EmailConfirmed}");
                
                var isAdmin = await _userHelper.IsUserInRoleAsync(adminByUsername, "Admin");
                Console.WriteLine($"   - IsAdmin: {isAdmin}");
            }
            else
            {
                Console.WriteLine("❌ Usuário 'admin' NÃO encontrado!");
            }
            
            return Content("Diagnóstico concluído. Verifique o console para detalhes.");
        }

        // =======================
        // LOGIN ADMINISTRATIVO
        // =======================
        [HttpGet]
        public async Task<IActionResult> AdminLogin()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Verificar se o usuário autenticado é Admin
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    var isAdmin = await _userHelper.IsUserInRoleAsync(user, "Admin");
                    Console.WriteLine($"🔍 Usuário já autenticado: {user.Email} (Admin: {isAdmin})");
                    
                    if (isAdmin)
                    {
                        Console.WriteLine($"🔄 Redirecionando Admin autenticado para: /Admin/Admin/Index");
                        return RedirectToAction("Index", "Admin", new { area = "Admin" });
                    }
                    else
                    {
                        Console.WriteLine($"❌ Usuário autenticado não é Admin: {user.Email}");
                        // Fazer logout do usuário não-admin
                        await _userHelper.LogoutAsync();
                        TempData["ErrorMessage"] = "Acesso restrito a administradores.";
                    }
                }
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            Console.WriteLine($"🔍 AdminLogin POST - Iniciando processo");
            Console.WriteLine($"   - Username: {model.Username}");
            Console.WriteLine($"   - Password: {(string.IsNullOrEmpty(model.Password) ? "VAZIA" : "FORNECIDA")}");
            Console.WriteLine($"   - RememberMe: {model.RememberMe}");
            Console.WriteLine($"   - ModelState.IsValid: {ModelState.IsValid}");
            
            if (ModelState.IsValid)
            {
                try
                {
                    Console.WriteLine($"🔍 Tentando login com UserHelper.LoginAsync...");
                    
                    // Usar UserHelper.LoginAsync para autenticação
                    var result = await _userHelper.LoginAsync(model);
                    
                    Console.WriteLine($"🔍 Resultado do login:");
                    Console.WriteLine($"   - Succeeded: {result.Succeeded}");
                    Console.WriteLine($"   - IsLockedOut: {result.IsLockedOut}");
                    Console.WriteLine($"   - IsNotAllowed: {result.IsNotAllowed}");
                    Console.WriteLine($"   - RequiresTwoFactor: {result.RequiresTwoFactor}");
                    
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"✅ Login bem-sucedido, resolvendo usuário...");
                        
                        // Resolver usuário após login bem-sucedido
                        var user = await _userHelper.GetUserByEmailAsync(model.Username)
                                  ?? await _userHelper.GetUserByUsernameAsync(model.Username);
                        
                        if (user != null)
                        {
                            Console.WriteLine($"✅ Usuário encontrado: {user.Email}");
                            
                            // Verificar se o usuário tem a role Admin
                            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                            Console.WriteLine($"🔍 Verificação de role Admin: {isAdmin}");
                            
                            if (isAdmin)
                            {
                                Console.WriteLine($"✅ Usuário é Admin - redirecionando para /Admin/Admin/Index");
                                
                                // Log successful admin login
                                await LogAccess(user, "AdminLogin");
                                
                                // Redirecionar para área Admin
                                return RedirectToAction("Index", "Admin", new { area = "Admin" });
                            }
                            else
                            {
                                Console.WriteLine($"❌ Usuário não é Admin - fazendo logout");
                                
                                // Fazer logout do usuário não-admin
                                await _userHelper.LogoutAsync();
                                ModelState.AddModelError(string.Empty, "Acesso restrito a administradores.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"❌ Usuário não encontrado após login");
                            ModelState.AddModelError(string.Empty, "Erro interno. Tente novamente.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ Login falhou");
                        
                        // Tratar diferentes tipos de falha
                        if (result.IsLockedOut)
                        {
                            Console.WriteLine($"   - Motivo: Conta bloqueada");
                            ModelState.AddModelError(string.Empty, "Conta bloqueada. Tente novamente mais tarde.");
                        }
                        else if (result.IsNotAllowed)
                        {
                            Console.WriteLine($"   - Motivo: Conta não confirmada");
                            ModelState.AddModelError(string.Empty, "Conta não confirmada. Verifique seu email.");
                        }
                        else
                        {
                            Console.WriteLine($"   - Motivo: Credenciais inválidas");
                            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log do erro para debugging
                    Console.WriteLine($"❌ Erro no AdminLogin: {ex.Message}");
                    Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                    ModelState.AddModelError(string.Empty, "Erro interno. Tente novamente.");
                }
            }
            else
            {
                Console.WriteLine($"❌ ModelState inválido:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }
            }

            Console.WriteLine($"🔄 Retornando View com ModelState");
            return View(model);
        }

        // =======================
        // SETTINGS (CONFIGURAÇÕES)
        // =======================
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new SettingsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.PhoneNumber = model.PhoneNumber;

                    var result = await _userHelper.UpdateUserAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Configurações atualizadas com sucesso!";
                        return RedirectToAction("Settings");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Senha alterada com sucesso!";
                        return RedirectToAction("Settings");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }

        // =======================
        // PROFILE
        // =======================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt,
                Active = user.Active
            };

            return View(model);
        }

        // =======================
        // PASSWORD RESET
        // =======================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var token = await _userHelper.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                
                var emailBody = $"Please reset your password by clicking <a href='{callbackUrl}'>here</a>.";
                
                await _emailService.SendEmailAsync(user.Email, "Reset Password", emailBody);
                
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = null)
        {
            if (token == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userHelper.GetUserByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}


