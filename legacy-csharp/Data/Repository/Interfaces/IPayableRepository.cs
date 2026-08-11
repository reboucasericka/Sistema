using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IPayableRepository : IGenericRepository<Payable>
    {
        // Listar payables com usuários
        IQueryable<Payable> GetAllWithUsers();
        
        // Listar payables por período
        IQueryable<Payable> GetByDateRange(DateTime startDate, DateTime endDate);
        
        // Listar payables pendentes
        IQueryable<Payable> GetPendingPayables();
        
        // Listar payables pagos
        IQueryable<Payable> GetPaidPayables();
        
        // Buscar payable por ID com includes
        Task<Payable?> GetByIdWithIncludesAsync(int id);
        
        // Marcar como pago
        Task<bool> MarkAsPaidAsync(int id, string userId);
    }
}
