using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Helpers;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProfessionalsController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IImageHelper _imageHelper;
        private readonly IUserHelper _userHelper;

        public AdminProfessionalsController(SistemaDbContext context, IImageHelper imageHelper, IUserHelper userHelper)
        {
            _context = context;
            _imageHelper = imageHelper;
            _userHelper = userHelper;
        }

        // GET: Profissionais
        public async Task<IActionResult> Index()
        {
            var sistemaDbContext = _context.Professionals.Include(p => p.User);
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
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProfessionalId == id);
            if (professional == null)
            {
                return NotFound();
            }

            return View(professional);
        }

        // GET: Profissionais/Create
        public async Task<IActionResult> Create()
        {
            // Carregar usuários existentes para o dropdown
            var users = await _context.Users
                .Where(u => u.Email != null)
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync();
            
            ViewData["Id"] = new SelectList(users, "Id", "Email");
            return View();
        }

        // POST: Profissionais/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Professional professional, IFormFile? photoFile, string? email, string? password)
        {
            try
            {
                // Upload da foto se fornecida
                if (photoFile != null && photoFile.Length > 0)
                {
                    professional.ImageId = Guid.Parse(await _imageHelper.UploadImageAsync(photoFile, "professionals") ?? Guid.Empty.ToString());
                }

                // Se email e senha foram fornecidos, criar usuário funcionário
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    var user = new User
                    {
                        FirstName = professional.Name.Split(' ')[0],
                        LastName = professional.Name.Contains(' ') ? string.Join(" ", professional.Name.Split(' ').Skip(1)) : "",
                        Email = email,
                        UserName = email,
                        PhoneNumber = "000000000",
                        EmailConfirmed = true
                    };

                    var result = await _userHelper.CreateEmployeeUserAsync(user, password);
                    if (result.Succeeded)
                    {
                        professional.UserId = user.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("", "Erro ao criar usuário: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                        await LoadUsersForDropdown();
                        return View(professional);
                    }
                }

                // Definir como ativo por padrão se não especificado
                if (!professional.IsActive)
                {
                    professional.IsActive = true;
                }

                _context.Add(professional);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Profissional criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao salvar profissional: " + ex.Message);
                await LoadUsersForDropdown();
                return View(professional);
            }
        }

        private async Task LoadUsersForDropdown()
        {
            var users = await _context.Users
                .Where(u => u.Email != null)
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync();
            
            ViewData["Id"] = new SelectList(users, "Id", "Email");
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
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", professional.UserId);
            return View(professional);
        }

        // POST: Profissionais/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Professional professional, IFormFile? photoFile)
        {
            if (id != professional.ProfessionalId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Upload da nova foto se fornecida
                    if (photoFile != null && photoFile.Length > 0)
                    {
                        professional.ImageId = Guid.Parse(await _imageHelper.UploadImageAsync(photoFile, "professionals") ?? Guid.Empty.ToString());
                    }

                    _context.Update(professional);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfessionalExists(professional.ProfessionalId))
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
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Email", professional.UserId);
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
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.ProfessionalId == id); 
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

        private bool ProfessionalExists(int id)
        {
            return _context.Professionals.Any(e => e.ProfessionalId == id);
        }
    }
}
