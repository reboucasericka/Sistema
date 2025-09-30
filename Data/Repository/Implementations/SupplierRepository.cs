using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        private readonly SistemaDbContext _context;

        public SupplierRepository(SistemaDbContext context) : base(context)
        {
            
            _context = context;
        }

        public IQueryable<Supplier> GetByName(string name)
        {
            return _context.Suppliers
                .Where(s => s.Name.Contains(name))
                .AsNoTracking();
        }

        public async Task<Supplier?> GetByIdWithProductsAsync(int id)
        {
            return await _context.Suppliers
                .Include(s => s.Products)
                .FirstOrDefaultAsync(s => s.SupplierId == id);
        }

       
    }
}
