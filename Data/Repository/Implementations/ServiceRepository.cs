using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class ServiceRepository : GenericRepository<Service>, IServiceRepository
    {
        private readonly SistemaDbContext _context;

        public ServiceRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Service> GetAllWithIncludes()
        {
            return _context.Service
                .Include(s => s.Category)
                .AsNoTracking();
        }

        public async Task<Service?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Service
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.ServiceId == id);
        }

        public IQueryable<Service> GetByCategory(int categoryId)
        {
            return _context.Service
                .Include(s => s.Category)
                .Where(s => s.CategoryId == categoryId)
                .AsNoTracking();
        }
    }
}