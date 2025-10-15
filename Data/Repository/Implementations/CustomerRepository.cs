using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        private readonly SistemaDbContext _context;

        public CustomerRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Customer> GetAllWithIncludes()
        {
            return _context.Customers
                .Include(c => c.User)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .AsNoTracking();
        }

        public IQueryable<Customer> GetActiveCustomers()
        {
            return _context.Customers
                .Where(c => c.IsActive)
                .Include(c => c.User)
                .AsNoTracking();
        }

        public async Task<Customer?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public IQueryable<Customer> GetByUserId(string userId)
        {
            return _context.Customers
                .Where(c => c.UserId == userId)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Professional)
                .Include(c => c.Appointments)
                    .ThenInclude(a => a.Service)
                .AsNoTracking();
        }
    }
}
