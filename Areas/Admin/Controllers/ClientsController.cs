using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly SistemaDbContext _context;

        public CustomersController(SistemaDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Customers
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            return View(clients);
        }

        // GET: Admin/Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Admin/Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CustomerId,Name,Email,Phone,Address,BirthDate,Notes,IsActive")] Customer client)
        {
            if (ModelState.IsValid)
            {
                client.RegistrationDate = DateTime.Now;
                client.IsActive = true;
                
                _context.Add(client);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Admin/Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Customers.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,Name,Email,Phone,Address,BirthDate,Notes,IsActive,RegistrationDate")] Customer client)
        {
            if (id != client.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Customer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(client.CustomerId))
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
            return View(client);
        }

        // GET: Admin/Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Customers.FindAsync(id);
            if (client != null)
            {
                _context.Customers.Remove(client);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Customer deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
    }
}
