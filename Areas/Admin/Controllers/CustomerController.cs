using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using Sistema.Models.Admin;
using Sistema.Models;
using Sistema.Helpers;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly SistemaDbContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly IUserHelper _userHelper;
        private readonly IImageHelper _imageHelper;

        public CustomerController(SistemaDbContext context, IConverterHelper converterHelper, IUserHelper userHelper, IImageHelper imageHelper)
        {
            _context = context;
            _converterHelper = converterHelper;
            _userHelper = userHelper;
            _imageHelper = imageHelper;
        }

        // GET: Admin/Customer
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["EmailSortParm"] = sortOrder == "email" ? "email_desc" : "email";
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["CurrentFilter"] = searchString;

            var customers = from c in _context.Customers
                           .Include(c => c.Appointments)
                           select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.Name.Contains(searchString) 
                                             || c.Email.Contains(searchString)
                                             || c.Phone.Contains(searchString));
            }

            customers = sortOrder switch
            {
                "name_desc" => customers.OrderByDescending(c => c.Name),
                "email" => customers.OrderBy(c => c.Email),
                "email_desc" => customers.OrderByDescending(c => c.Email),
                "date" => customers.OrderBy(c => c.RegistrationDate),
                "date_desc" => customers.OrderByDescending(c => c.RegistrationDate),
                _ => customers.OrderBy(c => c.Name),
            };

            int pageSize = 10;
            var paginatedCustomers = await PaginatedList<Customer>.CreateAsync(customers.AsNoTracking(), pageNumber ?? 1, pageSize);
            
            var customerViewModels = paginatedCustomers.Select(c => _converterHelper.ToCustomerViewModel(c)).ToList();

            return View(customerViewModels);
        }

        // GET: Admin/Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            var customerViewModel = _converterHelper.ToCustomerViewModel(customer);
            return View(customerViewModel);
        }

        // GET: Admin/Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (!string.IsNullOrEmpty(model.Email) && await _context.Customers.AnyAsync(c => c.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "A customer with this email already exists.");
                    return View(model);
                }

                // Process photo upload
                if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("PhotoFile", "Apenas arquivos JPG, PNG, GIF e WebP são permitidos.");
                        return View(model);
                    }

                    // Validate file size (2MB max)
                    if (model.PhotoFile.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("PhotoFile", "O tamanho do arquivo não pode exceder 2MB.");
                        return View(model);
                    }

                    model.PhotoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "customers");
                }

                var customer = _converterHelper.ToCustomer(model);
                _context.Add(customer);
                await _context.SaveChangesAsync();

                // Log the action
                await LogCustomerAction("Create", customer.CustomerId, customer.Name);

                TempData["SuccessMessage"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Admin/Customer/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var customerEditViewModel = _converterHelper.ToCustomerEditViewModel(customer);
            return View(customerEditViewModel);
        }

        // POST: Admin/Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerEditViewModel model)
        {
            if (id != model.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if email already exists for another customer
                    if (!string.IsNullOrEmpty(model.Email) && 
                        await _context.Customers.AnyAsync(c => c.Email == model.Email && c.CustomerId != id))
                    {
                        ModelState.AddModelError("Email", "A customer with this email already exists.");
                        return View(model);
                    }

                    var customer = await _context.Customers.FindAsync(id);
                    if (customer == null)
                    {
                        return NotFound();
                    }

                    // Update basic fields
                    customer.Name = model.Name;
                    customer.Email = model.Email;
                    customer.Phone = model.Phone;
                    customer.Address = model.Address;
                    customer.BirthDate = model.BirthDate;
                    customer.Notes = model.Notes;
                    customer.AllergyHistory = model.AllergyHistory;
                    customer.IsActive = model.IsActive;

                    // Process photo upload
                    if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                    {
                        // Validate file type
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                        var fileExtension = Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("PhotoFile", "Apenas arquivos JPG, PNG, GIF e WebP são permitidos.");
                            return View(model);
                        }

                        // Validate file size (2MB max)
                        if (model.PhotoFile.Length > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("PhotoFile", "O tamanho do arquivo não pode exceder 2MB.");
                            return View(model);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(customer.PhotoPath))
                        {
                            _imageHelper.DeleteImage(customer.PhotoPath, "customers");
                        }

                        // Upload new image
                        customer.PhotoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "customers");
                    }

                    _context.Update(customer);
                    await _context.SaveChangesAsync();

                    // Log the action
                    await LogCustomerAction("Edit", customer.CustomerId, customer.Name);

                    TempData["SuccessMessage"] = "Customer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(model.CustomerId))
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
            return View(model);
        }

        // GET: Admin/Customer/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Appointments)
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            var customerViewModel = _converterHelper.ToCustomerViewModel(customer);
            return View(customerViewModel);
        }

        // POST: Admin/Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // Check if customer has appointments
                var hasAppointments = await _context.Appointments.AnyAsync(a => a.CustomerId == id);
                if (hasAppointments)
                {
                    // Soft delete - mark as inactive instead of hard delete
                    customer.IsActive = false;
                    _context.Update(customer);
                    TempData["SuccessMessage"] = "Customer deactivated successfully! (Customer has appointments and cannot be deleted)";
                }
                else
                {
                    // Hard delete if no appointments
                    _context.Customers.Remove(customer);
                    TempData["SuccessMessage"] = "Customer deleted successfully!";
                }

                await _context.SaveChangesAsync();

                // Log the action
                await LogCustomerAction("Delete", customer.CustomerId, customer.Name);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Customer/Appointments/5
        public async Task<IActionResult> Appointments(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            var appointments = customer.Appointments
                .OrderByDescending(a => a.StartTime.Date)
                .ThenByDescending(a => a.StartTime.TimeOfDay)
                .ToList();

            ViewBag.CustomerName = customer.Name;
            return View(appointments);
        }

        // POST: Admin/Customer/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                customer.IsActive = !customer.IsActive;
                _context.Update(customer);
                await _context.SaveChangesAsync();

                // Log the action
                await LogCustomerAction("ToggleStatus", customer.CustomerId, customer.Name);

                TempData["SuccessMessage"] = $"Customer {(customer.IsActive ? "activated" : "deactivated")} successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        private async Task LogCustomerAction(string action, int customerId, string customerName)
        {
            try
            {
                var currentUser = await _userHelper.GetUserByEmailAsync(User.Identity?.Name ?? "");
                if (currentUser != null)
                {
                    var accessLog = new AccessLog
                    {
                        UserId = currentUser.Id,
                        Action = $"Customer_{action}",
                        Role = "Admin",
                        Email = currentUser.Email,
                        DateTime = DateTime.Now,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
                    };

                    _context.AccessLogs.Add(accessLog);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't break the main flow
                Console.WriteLine($"Error logging customer action: {ex.Message}");
            }
        }
    }
}
