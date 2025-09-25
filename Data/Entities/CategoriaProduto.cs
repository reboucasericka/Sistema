using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("CategoriasProdutos")]
    public class CategoriaProduto
    {
        [Key]
        public int CategoriaProdutoId { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; }

        // 🔗 relação 1:N (uma categoria pode ter vários produtos)
        public ICollection<Produto> Produtos { get; set; }
    }
}