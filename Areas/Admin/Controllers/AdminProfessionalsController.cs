using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;
using Sistema.Models.Admin;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProfessionalsController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IProfessionalRepository _professionalRepository;
        private readonly IBlobHelper _blobHelper;
        private readonly IUserHelper _userHelper;
        private readonly IStorageHelper _storageHelper;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminProfessionalsController(SistemaDbContext context,
            IProfessionalRepository professionalRepository,
            IBlobHelper blobHelper,
            IUserHelper userHelper,
            IStorageHelper storageHelper,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _professionalRepository = professionalRepository;
            _blobHelper = blobHelper;
            _userHelper = userHelper;
            _storageHelper = storageHelper;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Profissionais
        public async Task<IActionResult> Index()
        {
            var professionals = _professionalRepository.GetAllWithIncludes().OrderBy(p => p.Name);
            return View(await professionals.ToListAsync());
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
            
            ViewData["ExistingUsers"] = new SelectList(users, "Id", "Email");
            return View(new AdminProfessionalCreateViewModel());
        }

        // POST: Profissionais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProfessionalCreateViewModel model)
        {
            try
            {
                // Validação: deve ter usuário existente OU criar novo usuário
                if (string.IsNullOrEmpty(model.ExistingUserId) && 
                    (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password)))
                {
                    ModelState.AddModelError("", "Selecione um usuário existente ou crie um novo.");
                    await LoadUsersForDropdown();
                    return View(model);
                }

                string userId = string.Empty;

                // Se usuário existente foi selecionado
                if (!string.IsNullOrEmpty(model.ExistingUserId))
                {
                    userId = model.ExistingUserId;
                }
                // Se email e senha foram fornecidos, criar novo usuário
                else if (!string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(model.Password))
                {
                    var user = new User
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);
                        await LoadUsersForDropdown();
                        return View(model);
                    }

                    userId = user.Id;
                    await _userManager.AddToRoleAsync(user, "Professional");
                }

                // Upload da foto se fornecida
                Guid? imageId = null;
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    try
                    {
                        string photoPath = await _storageHelper.UploadAsync(model.PhotoFile, "professionals");
                        if (!string.IsNullOrEmpty(photoPath))
                        {
                            imageId = Guid.Parse(photoPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERRO no upload da imagem: {ex.Message}");
                        TempData["WarningMessage"] = "Imagem não foi enviada, profissional criado sem foto.";
                    }
                }

                // Criar o profissional
                var professional = new Professional
                {
                    Name = model.Name,
                    DefaultCommission = model.DefaultCommission,
                    Specialty = model.Specialty,
                    IsActive = model.IsActive,
                    UserId = userId,
                    ImageId = imageId ?? Guid.NewGuid()
                };

                _context.Add(professional);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Profissional criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Erro ao salvar profissional: " + ex.Message);
                await LoadUsersForDropdown();
                return View(model);
            }
        }

        private async Task LoadUsersForDropdown()
        {
            var users = await _context.Users
                .Where(u => u.Email != null)
                .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
                .ToListAsync();
            
            ViewData["ExistingUsers"] = new SelectList(users, "Id", "Email");
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
                        professional.ImageId = await _blobHelper.UploadBlobAsync(photoFile, "professionals");
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

        // POST: Profissionais/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var professional = await _context.Professionals.FindAsync(id);
            if (professional == null)
            {
                return NotFound();
            }

            professional.IsActive = !professional.IsActive;
            _context.Update(professional);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = professional.IsActive ? "Profissional ativado com sucesso!" : "Profissional desativado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        private bool ProfessionalExists(int id)
        {
            return _context.Professionals.Any(e => e.ProfessionalId == id);
        }
    }
}
