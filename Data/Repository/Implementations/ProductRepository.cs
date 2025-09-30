using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly SistemaDbContext _context;

        public ProductRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable GetAllWithUsers()
        {
            return _context.Products.Include(p => p.User);

        }



        public IQueryable<Product> GetAllWithIncludes()
        {
            return _context.Products
                .Include(p => p.ProductCategory)// ✅ navigation property
                //.Include(p => p.Supplier)
                .AsNoTracking();
        }

        public IQueryable<Product> GetByCategoria(int productCategoryId)
        {
            return _context.Products
                .Include(p => p.ProductCategory)  // navigation
                //.Include(p => p.Supplier)         // navigation
                .Where(p => p.ProductCategoryId == productCategoryId) // ✅ usa a FK
                .AsNoTracking();
        }

        public IQueryable<Product> GetBySupplier(int supplierId)
        {
            return _context.Products
                .Include(p => p.ProductCategory)
                //.Include(p => p.Supplier)
                .Where(p => p.SupplierId == supplierId)
                .AsNoTracking();
        }

        public async Task<Product?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductCategory)
                //.Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public IQueryable<Product> GetProductsWithLowStock()
        {
            return _context.Products
                .Include(p => p.ProductCategory)
                //.Include(p => p.Supplier)
                .Where(p => p.Stock < p.MinimumStockLevel)
                .AsNoTracking();
        }

        public IQueryable<Product> GetProdutosComEstoqueBaixo()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Product> GetByFornecedor(int fornecedorId)
        {
            throw new NotImplementedException();
        }
    }
}
