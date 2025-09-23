using Microsoft.AspNetCore.Mvc;
using Sistema.Models;

namespace Sistema.Controllers
{
    public class PerfilController : Controller
    {
        // GET: /Perfil/Editar
        public IActionResult Editar()
        {
            int userId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            // aqui buscaria do banco o usuário logado
            var usuario = new UsuarioModel { Id = userId, Nome = "Teste" };
            return View(usuario);
        }

        // POST: /Perfil/Editar
        [HttpPost]
        public IActionResult Editar(UsuarioModel model)
        {
            if (ModelState.IsValid)
            {
                // salvar no banco as alterações do perfil
                return RedirectToAction("Index", "Painel");
            }
            return View(model);
        }
    }
}