using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class PayableRepository : GenericRepository<Payable>, IPayableRepository
    {
        private readonly SistemaDbContext _context;

        public PayableRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Payable> GetAllWithUsers()
        {
            return _context.Payables
                .Include(p => p.LaunchUser)
                .Include(p => p.ClearUser)
                .Include(p => p.PaymentMethod)
                .AsNoTracking();
        }

        public IQueryable<Payable> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Payables
                .Include(p => p.LaunchUser)
                .Include(p => p.ClearUser)
                .Include(p => p.PaymentMethod)
                .Where(p => p.LaunchDate >= startDate && p.LaunchDate <= endDate)
                .AsNoTracking();
        }

        public IQueryable<Payable> GetPendingPayables()
        {
            return _context.Payables
                .Include(p => p.LaunchUser)
                .Include(p => p.PaymentMethod)
                .Where(p => !p.IsPaid)
                .AsNoTracking();
        }

        public IQueryable<Payable> GetPaidPayables()
        {
            return _context.Payables
                .Include(p => p.LaunchUser)
                .Include(p => p.ClearUser)
                .Include(p => p.PaymentMethod)
                .Where(p => p.IsPaid)
                .AsNoTracking();
        }

        public async Task<Payable?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Payables
                .Include(p => p.LaunchUser)
                .Include(p => p.ClearUser)
                .Include(p => p.PaymentMethod)
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.PayableId == id);
        }

        public async Task<bool> MarkAsPaidAsync(int id, string userId)
        {
            var payable = await _context.Payables.FindAsync(id);
            if (payable == null) return false;

            payable.IsPaid = true;
            payable.PaymentDate = DateTime.Now;
            payable.ClearUserId = userId;

            _context.Update(payable);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
