using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IReceivableRepository : IGenericRepository<Receivable>
    {
        // Listar receivables com usuários
        IQueryable<Receivable> GetAllWithUsers();
        
        // Listar receivables por período
        IQueryable<Receivable> GetByDateRange(DateTime startDate, DateTime endDate);
        
        // Listar receivables pendentes
        IQueryable<Receivable> GetPendingReceivables();
        
        // Listar receivables pagos
        IQueryable<Receivable> GetPaidReceivables();
        
        // Buscar receivable por ID com includes
        Task<Receivable?> GetByIdWithIncludesAsync(int id);
        
        // Marcar como pago
        Task<bool> MarkAsPaidAsync(int id, string userId);
    }
}
