using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IProductCategoryRepository : IGenericRepository<ProductCategory>
    {
        // Aqui podes adicionar métodos específicos se precisares no futuro.
        // Exemplo:
        // Task<IEnumerable<CategoriaProduto>> GetCategoriasComProdutosAsync();
    }
}
