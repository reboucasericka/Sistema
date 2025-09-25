using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IFornecedorRepository : IGenericRepository<Fornecedor>
    {
        // Aqui entram métodos específicos de Fornecedor, além do CRUD genérico.

        // Listar fornecedores por nome (pesquisa)
        IQueryable<Fornecedor> GetByName(string nome);

        // Trazer fornecedor com seus produtos
        Task<Fornecedor?> GetByIdWithProdutosAsync(int id);
    }
}
