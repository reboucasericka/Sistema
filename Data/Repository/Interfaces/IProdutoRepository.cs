using Sistema.Data.Entities;
using System.Collections.Generic;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IProdutoRepository : IGenericRepository<Produto>
    {
        // Listar todos os produtos já incluindo categoria e fornecedor
        IQueryable<Produto> GetAllWithIncludes();

        // Buscar 1 produto já com categoria e fornecedor
        Task<Produto?> GetByIdWithIncludesAsync(int id);

        // Buscar produtos por categoria
        IQueryable<Produto> GetByCategoria(int categoriaProdutoId);

        // Buscar produtos por fornecedor
        IQueryable<Produto> GetByFornecedor(int fornecedorId);

        // Verificar estoque abaixo do mínimo
        IQueryable<Produto> GetProdutosComEstoqueBaixo();
    }
}