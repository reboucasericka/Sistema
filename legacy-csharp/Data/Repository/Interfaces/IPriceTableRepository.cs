using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IPriceTableRepository : IGenericRepository<PriceTable>
    {
        IQueryable<PriceTable> GetAllOrdered();
        IQueryable<PriceTable> GetByCategory(string category);
    }
}