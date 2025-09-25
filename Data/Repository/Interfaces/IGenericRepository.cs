using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;


namespace Sistema.Data.Repository.Interfaces
{
    public interface IGenericRepository<T> where T : class, IEntity //todas as entidades terão Id e evita repetição de código para buscar pela PK.
    {
        
        
        IQueryable<T> GetAll();
        Task<T?> GetByIdAsync(int id);

        Task CreateAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<bool> ExistsAsync(int id);
    }
}

