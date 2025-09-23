using Microsoft.AspNetCore.Mvc;
using Sistema.Data;
using Sistema.Models;

namespace Sistema.Controllers
{
    public class CadastroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CadastroController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Salvar([FromForm] UsuarioModel usuario)
        {
            if (string.IsNullOrEmpty(usuario.Email) || string.IsNullOrEmpty(usuario.Senha))
            {
                return Content("Preencha todos os campos!");
            }

            // regra de negócio simplificada
            if (_context.Usuarios.Any(u => u.Email == usuario.Email))
            {
                return Content("Email já cadastrado!");
            }

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return Content("Cadastrado com Sucesso");
        }
    }
}