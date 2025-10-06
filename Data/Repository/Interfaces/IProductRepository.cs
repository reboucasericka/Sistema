using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Implementations;
using System.Collections.Generic;
using System.Linq;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {

        public IQueryable<Product> GetAllWithUsers(); //metodo novo para incluir o usuário na api

        // List all products including category and supplier
        IQueryable<Product> GetAllWithIncludes();

        // Get one product with category and supplier
        Task<Product?> GetByIdWithIncludesAsync(int id);

        // Get products by category
        IQueryable<Product> GetByCategory(int productCategoryId);

        // Get products by supplier
        IQueryable<Product> GetBySupplier(int supplierId);

        // Get products with low stock
        IQueryable<Product> GetProductsWithLowStock();
    }
}