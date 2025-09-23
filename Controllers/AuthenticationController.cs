using Microsoft.AspNetCore.Mvc;

namespace Sistema.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpPost]
        public IActionResult Login(string email, string senha)
        {
            // Exemplo simplificado (simula banco)
           
            var usuarioFake = new
            {
                Id = 1,
                Email = "admin@sistema.com",
                SenhaCrip = "202cb962ac59075b964b07152d234b70", // md5("123")
                Ativo = "Sim",
                Nivel = "Admin",
                Nome = "Administrador"
            };

            // criptografa a senha recebida em md5 (igual seu PHP)
            var senhaCrip = System.Security.Cryptography.MD5
                .Create()
                .ComputeHash(System.Text.Encoding.UTF8.GetBytes(senha));
            var senhaHex = BitConverter.ToString(senhaCrip).Replace("-", "").ToLower();

            // valida login
            if ((email == usuarioFake.Email || email == "12345678900") && senhaHex == usuarioFake.SenhaCrip)
            {
                if (usuarioFake.Ativo == "Sim")
                {
                    HttpContext.Session.SetInt32("id", usuarioFake.Id);
                    HttpContext.Session.SetString("nivel", usuarioFake.Nivel);
                    HttpContext.Session.SetString("nome", usuarioFake.Nome);

                    // Redireciona para o painel (PainelController.Index)
                    return RedirectToAction("Index", "Painel");
                }
                else
                {
                    TempData["Erro"] = "Usuário desativado, contate o administrador!";
                    return RedirectToAction("Login");
                }
            }
            else
            {
                TempData["Erro"] = "Usuário ou senha incorretos!";
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Erro = TempData["Erro"];
            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // limpa sessão
            return RedirectToAction("Login", "Authentication"); // volta para tela de login
        }
    }
}