using Microsoft.AspNetCore.Mvc;
using Sistema.Data;

namespace Sistema.Controllers
{
    public class PainelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PainelController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var idUsuario = HttpContext.Session.GetInt32("UsuarioId");
            if (idUsuario == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == idUsuario);

            if (usuario == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Authentication");
            }

            // Preenche o ViewBag para o layout usar
            ViewBag.NomeUsuario = usuario.Nome;
            ViewBag.NivelUsuario = usuario.Nivel;
            ViewBag.FotoUsuario = string.IsNullOrEmpty(usuario.Foto) ? "sem-foto.jpg" : usuario.Foto;

            return View(usuario); // aqui vai para Views/Painel/Index.cshtml
        }
    }
}