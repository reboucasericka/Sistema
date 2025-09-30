using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("ProductCategories")]
    public class ProductCategory : IEntity
    {
        [Key]
        public int ProductCategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        // 🔗 relação 1:N (uma categoria pode ter vários produtos)
        public ICollection<Product> Products { get; set; }

        // ✅ Implementação da interface IEntity
        public int Id
        {
            get => ProductCategoryId;
            set => ProductCategoryId = value;
        }
    }
}