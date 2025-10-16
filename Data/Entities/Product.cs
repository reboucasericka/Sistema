using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

namespace Sistema.Data.Entities
{
    [Table("Products")]
    public class Product : IEntity
    {
        [Key]
        public int ProductId { get; set; } //ptodutoId

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty; //Nome

        [MaxLength(255)]
        public string? Description { get; set; } //Descrição

      

        // 🔗 FK → ProductCategories
        [ForeignKey("ProductCategory")]
        [Range(1, int.MaxValue, ErrorMessage = "A categoria é obrigatória")]
        public int ProductCategoryId { get; set; } //CategoriaId
        public ProductCategory ProductCategory { get; set; } = null!; //navegação

        [Required, Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)] 
        public decimal PurchasePrice { get; set; } //valorCompra

        [Required, Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal SalePrice { get; set; } //valorVenda

        public int Stock { get; set; } = 0; //quantidade em stock

        
        [Display(Name = "Foto do Produto")]
        public Guid? ImageId { get; set; } // Foto do produto (caminho da imagem)

        public int MinimumStockLevel { get; set; } = 0; //StockMinimo

        public bool IsActive { get; set; } = true; // Indica se o produto está ativo

        // 🔗 FK → Suppliers (optional)
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; } //FornecedorId (pode ser null)
        public Supplier? Supplier { get; set; }

        // 🔗 FK → User (quem criou o produto)
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public User? User { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/noimage.png"
            : $"/uploads/products/{ImageId}.png";


    }
}
