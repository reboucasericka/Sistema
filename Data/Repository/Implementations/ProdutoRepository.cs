using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sistema.Data.Repository.Implementations
{
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly SistemaDbContext _context;

        public ProdutoRepository(SistemaDbContext context)
        {
            _context = context;
        }

        //PRODUTOS
        public IEnumerable<Produto> GetAllProducts()
        {
            return _context.Produtos
                .Include(p => p.CategoriaProduto)
                .Include(p => p.Fornecedor)
                .OrderBy(p => p.Nome)
                .ToList();
        }
        public Produto? GetProductById(int id, bool incluirRelacionamentos = false)
        {
            if (incluirRelacionamentos)
            {
                return _context.Produtos
                    .Include(p => p.CategoriaProduto)
                    .Include(p => p.Fornecedor)
                    .FirstOrDefault(p => p.ProdutoId == id);
            }

            return _context.Produtos.Find(id);
        }
        public void AddProduct(Produto produto)
        {
            _context.Produtos.Add(produto);

        }
        public void UpdateProduct(Produto produto)
        {
            _context.Produtos.Update(produto);
        }
        public void RemoveProduct(Produto produto)
        {
            _context.Produtos.Remove(produto);
        }        
        public bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(p => p.ProdutoId == id);
        }
        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }       
        
    }
}
