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
        // LOGIN CLIENTE AdminController, dentro da área "Admin", com um método Index()
        // =======================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userHelper.LoginAsync(model);

            if (result.Succeeded)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username)
                          ?? await _userHelper.GetUserByUsernameAsync(model.Username);
                // Se existir ReturnUrl, respeita
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return LocalRedirect(returnUrl);

                // Caso contrário, decide pela role
                if (user != null && await _userHelper.IsUserInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return View(model);
        }/*
    
                if (user != null)
                {
                    // Respeita o ReturnUrl se for válido
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return LocalRedirect(returnUrl);

                    // Caso contrário, decide pela role
                    if (user != null && await _userHelper.IsUserInRoleAsync(user, "Admin"))
                        return RedirectToAction("Index", "Admin", new { area = "Admin" });

                    // Senão, redireciona com base na role
                    //if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
                       // return RedirectToAction("Index", "Admin", new { area = "Admin" });

                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            return View(model);
        }*/


        // =======================
        // LOGIN ADMINISTRATIVO
        // =======================
        [HttpGet]
        public async Task<IActionResult> AdminLogin()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    var isAdmin = await _userHelper.IsUserInRoleAsync(user, "Admin");
                    if (isAdmin)
                        return RedirectToAction("Index", "Admin", new { area = "Admin" });

                    await _userHelper.LogoutAsync();
                    TempData["ErrorMessage"] = "Acesso restrito a administradores.";
                }
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userHelper.LoginAsync(model);

            if (result.Succeeded)
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username)
                          ?? await _userHelper.GetUserByUsernameAsync(model.Username);

                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await LogAccess(user, "AdminLogin");
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                }

                await _userHelper.LogoutAsync();
                ModelState.AddModelError(string.Empty, "Acesso restrito a administradores.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            }

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
                return Redirect("/Account/Login");
            }

            // Pega as informações do login externo
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Redirect("/Account/Login");
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
                return Redirect("/Account/Login");
            }
        }




        // =======================
        // LOGOUT
        // =======================
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete(".AspNetCore.Identity.Application");
            return RedirectToAction("Login", "Account");

        }



        // =======================
        // REGISTRO CLIENTE
        // =======================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid) //verifica se o modelo e valido
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Email); //verifica se o user ja existe
                if (user == null) //se nao existir cria um novo user
                {
                    user = new User //cria um novo user
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        UserName = model.Username,
                        PhoneNumber = model.Phone
                    };

                    var result = await _userHelper.AddUserAsync(user, model.Password);
                    if (result != IdentityResult.Success)  //se nao for possivel criar o user
                    {
                        ModelState.AddModelError(string.Empty, "Não foi possível criar o usuário. Tente novamente.");
                        return View(model); //retorna a view com o modelo
                    }
                    // Log do novo registro
                    var loginViewModel = new LoginViewModel
                    {
                        Password = model.Password, //do utilizador que ja esta criado
                        RememberMe = false, //nao se lembra do user deixar false
                        Username = model.Username //do utilizador que ja esta criado
                    };
                    var result2 = await _userHelper.LoginAsync(loginViewModel); //faz o login do user que acabou de criar
                    if (result2.Succeeded) //se ele conseguiu logar retorna de uma pagina para a home
                    {
                        return Redirect("/Home/Index");
                    }

                    ModelState.AddModelError(string.Empty, "The user couldn't be logged in."); //se ele nao conseguir logar da mensagem de erro


                }
            }
            return View(model); //se o modelo nao for valido retorna a view com o modelo
        }


                        /*
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
                        return Redirect("/Account/Login");
                }

                ModelState.AddModelError(string.Empty, "Já existe um usuário com este e-mail.");
            }

            return View(model);
        }
*/


        // =======================
        // ATIVAR CONTA
        // =======================
        public async Task<IActionResult> ActivateAccount(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Link de ativação inválido.";
                return Redirect("/Account/Login");
            }

            var user = await _userHelper.GetUserByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Usuário não encontrado.";
                return Redirect("/Account/Login");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Log successful account activation
                await LogAccess(user, "ActivateAccount");
                
                TempData["SuccessMessage"] = "Conta ativada com sucesso! Agora você pode entrar.";
                
                // Redirect based on user role
                if (await _userHelper.IsUserInRoleAsync(user, "Admin"))
                    return Redirect("/Admin/Admin");
                else if (await _userHelper.IsUserInRoleAsync(user, "Customer"))
                    return Redirect("/Public/PublicAppointment/Index");
                else
                    return Redirect("/Public/PublicAppointment/Index"); // Default to Customer area
            }

            TempData["ErrorMessage"] = "Erro ao ativar a conta. Link pode ter expirado.";
            return Redirect("/Account/Login");
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
        // PROFILE
        // =======================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Account/Login");
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            var model = new AdminProfileViewModel
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
                return Redirect("/Account/Login");
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


        // =======================
        // CHANGE USER
        // =======================
        public async Task<IActionResult> ChangeUser()
        {
            var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); //vai buscar o user pelo email
            var model = new ChangeUserViewModel(); //cria um novo modelo
            if (user == null) //se o user for nulo
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
            }
            return View(model); //retorna a view com o modelo
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUser(ChangeUserViewModel model)
        {
            if (ModelState.IsValid) //verifica se o modelo e valido
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); //vai buscar o user pelo email
                if (user != null) //se o user for nulo
                {
                    user.FirstName = model.FirstName; //atualiza o primeiro nome
                    user.LastName = model.LastName; //atualiza o ultimo nome
                    var response = await _userHelper.UpdateUserAsync(user); //atualiza o user

                    if (response.Succeeded) //se o resultado for sucesso
                    {
                        ViewBag.UserMessage = "User updated!";
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault().Description); //se nao conseguir atualizar o user da mensagem de erro
                    }
                }
            }
            return View(model); //retorna a view com o modelo
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
        // SETTINGS (CONFIGURAÇÕES)
        // =======================
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Account/Login");
            }

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Redirect("/Account/Login");
            }

            var model = new AdminSettingsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(AdminSettingsViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Redirect("/Account/Login");
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
    }
}