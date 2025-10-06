using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class PriceTableRepository : GenericRepository<PriceTable>, IPriceTableRepository
    {
        private readonly SistemaDbContext _context;

        public PriceTableRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<PriceTable> GetAllOrdered()
        {
            return _context.PriceTables
                .OrderBy(p => p.Category)
                .ThenBy(p => p.ServiceName)
                .AsNoTracking();
        }


        public IQueryable<PriceTable> GetByCategory(string category)
        {
            return _context.PriceTables
                .Where(p => p.Category == category)
                .AsNoTracking();
        }
    }
}
