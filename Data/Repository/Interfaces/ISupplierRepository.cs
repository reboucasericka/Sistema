using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        // Aqui entram métodos específicos de Fornecedor, além do CRUD genérico.

        // Listar fornecedores por nome (pesquisa)
        IQueryable<Supplier> GetByName(string nome);

        // Trazer fornecedor com seus produtos
        Task<Supplier?> GetByIdWithProductsAsync(int id);
    }
}
