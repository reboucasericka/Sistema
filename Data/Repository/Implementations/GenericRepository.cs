using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Data.Repository.Implementations
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class, IEntity
    {
        private SistemaDbContext _context;

        public GenericRepository(SistemaDbContext context)
        {
            _context = context;
        }

        public IQueryable<T> GetAll()
        {
            return _context.Set<T>().AsNoTracking();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var keyProperty = GetKeyProperty();
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, keyProperty);
            var constant = System.Linq.Expressions.Expression.Constant(id);
            var equality = System.Linq.Expressions.Expression.Equal(property, constant);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equality, parameter);
            
            return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(lambda);
        }

        public async Task CreateAsync(T entity)
        {
            try
            {
                await _context.Set<T>().AddAsync(entity);
                await SaveAllAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar entidade: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            await SaveAllAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            await SaveAllAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            var keyProperty = GetKeyProperty();
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, keyProperty);
            var constant = System.Linq.Expressions.Expression.Constant(id);
            var equality = System.Linq.Expressions.Expression.Equal(property, constant);
            var lambda = System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(equality, parameter);
            
            return await _context.Set<T>().AnyAsync(lambda);
        }

        private string GetKeyProperty()
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any())
                .ToList();

            if (properties.Count == 0)
            {
                throw new InvalidOperationException($"Entidade {typeof(T).Name} não possui propriedade com atributo [Key]");
            }

            return properties.First().Name;
        }

        private async Task<bool> SaveAllAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar no banco de dados: {ex.Message}", ex);
            }
        }              
    }
}