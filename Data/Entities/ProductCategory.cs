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

        // 🔗 1:N relationship (one category can have multiple products)
        public ICollection<Product> Products { get; set; }

        // ✅ IEntity interface implementation
        public int Id
        {
            get => ProductCategoryId;
            set => ProductCategoryId = value;
        }
    }
}