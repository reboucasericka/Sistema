using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class CategoriaProdutoRepository : GenericRepository<CategoriaProduto>, ICategoriaProdutoRepository
    {
        private readonly SistemaDbContext _context;

        public CategoriaProdutoRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }
        // Aqui podes adicionar métodos específicos se precisares.
    }
}
