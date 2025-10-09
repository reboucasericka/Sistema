using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph.Models;
using Sistema.Data;
using Sistema.Data.Entities;
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

        public AdminProfessionalSchedulesController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: ProfessionalSchedules
        public async Task<IActionResult> Index()
        {
            var schedule = _context.ProfessionalSchedules
                .Include(p => p.Professional)
                .ThenInclude(p => p.User);
            return View(await schedule.ToListAsync());
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
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Specialty");
            return View();
        }

        // POST: ProfessionalSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProfessionalSchedule professionalSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(professionalSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
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
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
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
            ViewData["ProfessionalId"] = new SelectList(_context.Professionals, "ProfessionalId", "Name", professionalSchedule.ProfessionalId);
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
