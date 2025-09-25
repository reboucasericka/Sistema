using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class FornecedorRepository : GenericRepository<Fornecedor>, IFornecedorRepository
    {
        private readonly SistemaDbContext _context;

        public FornecedorRepository(SistemaDbContext context) : base(context)
        {
            
            _context = context;
        }

        public IQueryable<Fornecedor> GetByName(string nome)
        {
            return _context.Fornecedores
                .Where(f => f.Nome.Contains(nome))
                .AsNoTracking();
        }

        public async Task<Fornecedor?> GetByIdWithProdutosAsync(int id)
        {
            return await _context.Fornecedores
                .Include(f => f.Produtos)
                .FirstOrDefaultAsync(f => f.FornecedorId == id);
        }
    }
}
