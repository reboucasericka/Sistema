using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Suppliers")]
    public class Supplier : IEntity
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? TaxId { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public int? DeliveryTime { get; set; } // in days

        public string? Notes { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // 🔗 1:N relationship (one supplier can provide multiple products)
        public ICollection<Product> Products { get; set; }

        // ✅ IEntity interface implementation
        public int Id
        {
            get => SupplierId; // returns PK value
            set => SupplierId = value; // sets PK value
        }
    }
}