using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class ProductCategoryRepository : GenericRepository<ProductCategory>, IProductCategoryRepository
    {
        private readonly SistemaDbContext _context;

        public ProductCategoryRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }
        // Aqui podes adicionar métodos específicos se precisares.
    }
}
