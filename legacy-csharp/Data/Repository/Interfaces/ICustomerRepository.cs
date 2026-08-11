using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        IQueryable<Customer> GetAllWithIncludes();
        IQueryable<Customer> GetActiveCustomers();
        Task<Customer?> GetByIdWithIncludesAsync(int id);
        IQueryable<Customer> GetByUserId(string userId);
    }
}
