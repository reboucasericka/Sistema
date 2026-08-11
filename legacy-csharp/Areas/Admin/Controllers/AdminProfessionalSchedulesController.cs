using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using Sistema.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProfessionalSchedulesController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IProfessionalScheduleRepository _professionalScheduleRepository;

        public AdminProfessionalSchedulesController(SistemaDbContext context, IProfessionalScheduleRepository professionalScheduleRepository)
        {
            _context = context;
            _professionalScheduleRepository = professionalScheduleRepository;
        }

        // GET: ProfessionalSchedules
        public async Task<IActionResult> Index()
        {
            var schedules = _professionalScheduleRepository.GetAllWithIncludes().OrderBy(ps => ps.Professional.Name).ThenBy(ps => ps.DayOfWeek);
            return View(await schedules.ToListAsync());
        }

        // GET: ProfessionalSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professionalSchedule = await _context.ProfessionalSchedules
                .Include(p => p.Professional)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (professionalSchedule == null)
            {
                return NotFound();
            }

            return View(professionalSchedule);
        }

        // GET: ProfessionalSchedules/Create
        public IActionResult Create()
        {
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals.Where(p => p.IsActive), "ProfessionalId", "Name");
            return View();
        }

        // POST: ProfessionalSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfessionalSchedule professionalSchedule)
        {
            Console.WriteLine("=== INÍCIO DO MÉTODO CREATE SCHEDULE (POST) ===");
            Console.WriteLine($"Schedule recebido - ProfissionalId: {professionalSchedule.ProfessionalId}, Dia: {professionalSchedule.DayOfWeek}");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(professionalSchedule);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Horário salvo com sucesso no banco de dados!");
                    TempData["SuccessMessage"] = "Horário criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERRO ao criar horário: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    }
                    TempData["ErrorMessage"] = $"Erro ao criar horário: {ex.Message}";
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                Console.WriteLine($"Erro de validação: {errorMessage}");
                TempData["ErrorMessage"] = errorMessage;
            }
            
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals.Where(p => p.IsActive), "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
            return View(professionalSchedule);
        }


        // GET: ProfessionalSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professionalSchedule = await _context.ProfessionalSchedules.FindAsync(id);
            if (professionalSchedule == null)
            {
                return NotFound();
            }
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals.Where(p => p.IsActive), "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
            return View(professionalSchedule);
        }

        // POST: ProfessionalSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  ProfessionalSchedule professionalSchedule)
        {
            if (id != professionalSchedule.ScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(professionalSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfessionalScheduleExists(professionalSchedule.ScheduleId))
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
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals.Where(p => p.IsActive), "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
            return View(professionalSchedule);
        }

        // GET: ProfessionalSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var professionalSchedule = await _context.ProfessionalSchedules
                .Include(p => p.Professional)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(m => m.ScheduleId == id);
            if (professionalSchedule == null)
            {
                return NotFound();
            }

            return View(professionalSchedule);
        }

        // POST: ProfessionalSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var professionalSchedule = await _context.ProfessionalSchedules.FindAsync(id);
            if (professionalSchedule != null)
            {
                _context.ProfessionalSchedules.Remove(professionalSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfessionalScheduleExists(int id)
        {
            return _context.ProfessionalSchedules.Any(e => e.ScheduleId == id);
        }
    }
}
