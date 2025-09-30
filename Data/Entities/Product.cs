using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;
using static System.Net.Mime.MediaTypeNames;

namespace Sistema.Data.Entities
{
    [Table("Produtos")]
    public class Product : IEntity
    {
        [Key]
        public int ProductId { get; set; } //ptodutoId

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } //Nome

        [MaxLength(255)]
        public string? Description { get; set; } //Descrição

        // Implementação da interface IEntity
        public int Id
        {
            get => ProductId; //retorna o valor da PK
            set => ProductId = value; //define o valor da PK
        }

        // 🔗 FK → ProductCategories
        [ForeignKey("ProductCategory")]
        public int ProductCategoryId { get; set; } //CategoriaId
        public ProductCategory ProductCategory { get; set; } //navegação

        [Required, Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)] 
        public decimal PurchasePrice { get; set; } //valorCompra

        [Required, Column(TypeName = "decimal(10,2)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
        public decimal SalePrice { get; set; } //valorVenda

        public int Stock { get; set; } = 0; //quantidade em stock

        [MaxLength(200)]
        [Display(Name = "Foto do Produto")]
        public Guid ImageId { get; set; }  //ImageProductFile

        public int MinimumStockLevel { get; set; } = 0; //StockMinimo

        // 🔗 FK → Suppliers (optional)
        [ForeignKey("Supplier")]
        public int? SupplierId { get; set; } //FornecedorId (pode ser null)
        public Supplier Supplier { get; set; }


        public User User { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://sistema.azurewebsites.net/images/noimage.png"
            : $"https://sistemacontaarmazenamen.blob.core.windows.net/images/Products/{ImageId}";       


    }
}
