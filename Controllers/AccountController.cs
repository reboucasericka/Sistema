using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Entities;
using Sistema.Helpers;
using Sistema.Models;

namespace Sistema.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;

        public AccountController(IUserHelper userHelper)
        {
            _userHelper = userHelper;
        }

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();

        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    if (this.Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(this.Request.Query["ReturnUrl"].First());
                    }
                    return this.RedirectToAction("Index", "Home");

                }
            }

            this.ModelState.AddModelError(string.Empty, "Failed to login");
            return View(model);

        }
        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");

        }
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid) //verifica se o modelo e valido
            {
                var user = await _userHelper.GetUserByEmailAsync(model.Username); //verifica se o user ja existe
                if (user == null)//se nao existir cria um novo user
                {
                    user = new User //cria um novo user
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Username,
                        UserName = model.Username
                    };
                    var result = await _userHelper.AddUserAsync(user, model.Password); //pede dois paramentros o user e a password
                    if (result != IdentityResult.Success) //se nao conseguir criar o user 
                    {
                        ModelState.AddModelError(string.Empty, "The user couldn't be created."); //
                        return View(model); //retorna a view com o modelo
                    }


                    var loginViewModel = new LoginViewModel
                    {
                        Password = model.Password, //do utilizador que ja esta criado
                        RememberMe = false, //nao se lembra do user deixar false
                        Username = model.Username //do utilizador que ja esta criado
                    };

                    var result2 = await _userHelper.LoginAsync(loginViewModel); //faz o login do user que acabou de criar
                    if (result2.Succeeded) //se ele conseguiu logar retorna de uma pagina para a home
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError(string.Empty, "The user couldn't be logged in."); //se ele nao conseguir logar da mensagem de erro


                }
            }
            return View(model); //se o modelo nao for valido retorna a view com o modelo
        }
        
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
                        ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault()?.Description); //se nao conseguir atualizar o user da mensagem de erro
                    }
                }
            }
            return View(model); //retorna a view com o modelo
        }


        public IActionResult ChangePassword() //metodo para mudar a password
        {
            return View(); //retorna a view nao faz mais nada 
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model) //metodo para mudar a password
        {
            if (ModelState.IsValid) //verifica se o modelo e valido
            {
                var user = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); //vai buscar o user pelo email
                if (user != null) //se o user for nulo significa que existe vai mudar o password
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword); //muda a password do user
                    if (result.Succeeded) //se o resultado for sucesso
                    {
                        return this.RedirectToAction("ChangeUser"); //redireciona para a pagina de mudar o user
                    }
                    else //caso nao consiga mudar a password
                    {
                        this.ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault()?.Description); //se nao conseguir mudar a password da mensagem de erro
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "User no found."); //se o user for nulo da mensagem de erro
                }
            }
            return View(model); //retorna a view com o modelo
        }

        public IActionResult NotAuthorized() //metodo para quando o user nao tiver autorizacao
        {
            return View(); //retorna a view
        }
    }
}

