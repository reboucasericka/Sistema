using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface ICategoriaProdutoRepository : IGenericRepository<CategoriaProduto>
    {
        // Aqui podes adicionar métodos específicos se precisares no futuro.
        // Exemplo:
        // Task<IEnumerable<CategoriaProduto>> GetCategoriasComProdutosAsync();
    }
}
