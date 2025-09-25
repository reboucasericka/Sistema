using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IProdutoRepository
    {

        // =========================
        // PRODUTOS
        // =========================
        IEnumerable<Produto> GetAllProducts();
        Produto? GetProductById(int id, bool incluirRelacionamentos = false);
        void AddProduct(Produto produto);
        void UpdateProduct(Produto produto);
        void RemoveProduct(Produto produto);
        bool ProdutoExists(int id);

        
        Task<bool> SaveAllAsync();

    }
}