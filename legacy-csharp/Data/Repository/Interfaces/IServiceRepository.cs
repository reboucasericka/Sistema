using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IServiceRepository : IGenericRepository<Service>
    {
        IQueryable<Service> GetAllWithIncludes();
        Task<Service?> GetByIdWithIncludesAsync(int id);
        IQueryable<Service> GetByCategory(int serviceCategoryId);
    }
}