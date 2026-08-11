using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("SaleItems")]
    public class SaleItem : IEntity
    {
        [Key]
        public int SaleItemId { get; set; }
        
        

        // 🔗 FK → Sale
        public int SaleId { get; set; }
        public Sale Sale { get; set; }

        // 🔗 FK → Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
