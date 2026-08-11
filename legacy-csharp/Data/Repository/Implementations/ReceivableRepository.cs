using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class ReceivableRepository : GenericRepository<Receivable>, IReceivableRepository
    {
        private readonly SistemaDbContext _context;

        public ReceivableRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Receivable> GetAllWithUsers()
        {
            return _context.Receivables
                .Include(r => r.LaunchUser)
                .Include(r => r.ClearUser)
                .Include(r => r.PaymentMethod)
                .AsNoTracking();
        }

        public IQueryable<Receivable> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Receivables
                .Include(r => r.LaunchUser)
                .Include(r => r.ClearUser)
                .Include(r => r.PaymentMethod)
                .Where(r => r.LaunchDate >= startDate && r.LaunchDate <= endDate)
                .AsNoTracking();
        }

        public IQueryable<Receivable> GetPendingReceivables()
        {
            return _context.Receivables
                .Include(r => r.LaunchUser)
                .Include(r => r.PaymentMethod)
                .Where(r => !r.IsPaid)
                .AsNoTracking();
        }

        public IQueryable<Receivable> GetPaidReceivables()
        {
            return _context.Receivables
                .Include(r => r.LaunchUser)
                .Include(r => r.ClearUser)
                .Include(r => r.PaymentMethod)
                .Where(r => r.IsPaid)
                .AsNoTracking();
        }

        public async Task<Receivable?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Receivables
                .Include(r => r.LaunchUser)
                .Include(r => r.ClearUser)
                .Include(r => r.PaymentMethod)
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.ReceivableId == id);
        }

        public async Task<bool> MarkAsPaidAsync(int id, string userId)
        {
            var receivable = await _context.Receivables.FindAsync(id);
            if (receivable == null) return false;

            receivable.IsPaid = true;
            receivable.PaymentDate = DateTime.Now;
            receivable.ClearUserId = userId;

            _context.Update(receivable);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
