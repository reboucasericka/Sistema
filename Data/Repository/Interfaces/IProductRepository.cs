using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Implementations;
using System.Collections.Generic;
using System.Linq;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {

        public IQueryable GetAllWithUsers(); //metodo novo para incluir o usuário na api

        // Listar todos os produtos já incluindo categoria e fornecedor
        IQueryable<Product> GetAllWithIncludes();

        // Buscar 1 produto já com categoria e fornecedor
        Task<Product?> GetByIdWithIncludesAsync(int id);

        // Buscar produtos por categoria
        IQueryable<Product> GetByCategoria(int categoriaProdutoId);

        // Buscar produtos por fornecedor
        IQueryable<Product> GetByFornecedor(int fornecedorId);

        // Verificar estoque abaixo do mínimo
        IQueryable<Product> GetProdutosComEstoqueBaixo();
    }
}