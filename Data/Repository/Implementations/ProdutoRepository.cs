using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class ProdutoRepository : GenericRepository<Produto>, IProdutoRepository
    {
        private readonly SistemaDbContext _context;

        public ProdutoRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }



        public IQueryable<Produto> GetAllWithIncludes()
        {
            return _context.Produtos
                .Include(p => p.CategoriaProduto)
                .Include(p => p.Fornecedor)
                .AsNoTracking();
        }

        public IQueryable<Produto> GetByCategoria(int categoriaProdutoId)
        {
            return _context.Produtos
                .Include(p => p.CategoriaProduto)
                .Include(p => p.Fornecedor)
                .Where(p => p.CategoriaProdutoId == categoriaProdutoId)
                .AsNoTracking();
        }

        public IQueryable<Produto> GetByFornecedor(int fornecedorId)
        {
            return _context.Produtos
                .Include(p => p.CategoriaProduto)
                .Include(p => p.Fornecedor)
                .Where(p => p.FornecedorId == fornecedorId)
                .AsNoTracking();
        }

        public async Task<Produto?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.CategoriaProduto)
                .Include(p => p.Fornecedor)
                .FirstOrDefaultAsync(p => p.ProdutoId == id);
        }

        public IQueryable<Produto> GetProdutosComEstoqueBaixo()
        {
            return _context.Produtos
                .Include(p => p.CategoriaProduto)
                .Include(p => p.Fornecedor)
                .Where(p => p.Estoque < p.NivelEstoqueMinimo)
                .AsNoTracking();
        }
    }
}
