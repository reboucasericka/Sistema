using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProfessionalsController : Controller
    {
        private readonly SistemaDbContext _context;

        public ProfessionalsController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Profissionais
        public async Task<IActionResult> Index()
        {
            var sistemaDbContext = _context.Professionals.Include(p => p.UsuarioId);
            return View(await sistemaDbContext.ToListAsync());
        }

        // GET: Profissionais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _context.Professionals
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ProfissionalId == id);
            if (professional == null)
            {
                return NotFound();
            }

            return View(professional);
        }

        // GET: Profissionais/Create
        public IActionResult Create()
        {
            ViewData["UsuarioId"] = new SelectList(_context.Users, "UserId", "Email");
            return View();
        }

        // POST: Profissionais/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProfissionalId,UsuarioId,Especialidade,ComissaoPadrao,Ativo")] Professional professional)
        {
            if (ModelState.IsValid)
            {
                _context.Add(professional);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Users, "UserId", "Email", professional.UsuarioId);
            return View(professional);
        }

        // GET: Profissionais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _context.Professionals.FindAsync(id);
            if (professional == null)
            {
                return NotFound();
            }
            ViewData["UsuarioId"] = new SelectList(_context.Users, "UserId", "Email", professional.UsuarioId);
            return View(professional);
        }

        // POST: Profissionais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProfissionalId,UsuarioId,Especialidade,ComissaoPadrao,Ativo")] Professional professional)
        {
            if (id != professional.ProfissionalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(professional);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfissionalExists(professional.ProfissionalId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UsuarioId"] = new SelectList(_context.Users, "UserId", "Email", professional.UsuarioId);
            return View(professional);
        }

        // GET: Profissionais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professional = await _context.Professionals
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(m => m.ProfissionalId == id); //preciso ajustar
            if (professional == null)
            {
                return NotFound();
            }

            return View(professional);
        }

        // POST: Profissionais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professional = await _context.Professionals.FindAsync(id);
            if (professional != null)
            {
                _context.Professionals.Remove(professional);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfissionalExists(int id)
        {
            return _context.Professionals.Any(e => e.ProfissionalId == id);
        }
    }
}
