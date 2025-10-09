using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Billings")]
    public class Billing :IEntity
    {
        [Key]
        [Column("BillingId")]  //  coluna real no SQL
        public int Id { get; set; }   // cumpre IEntity, mas mapeia pra BillingId

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalValue { get; set; }

        public int ServicesQuantity { get; set; } = 0;

        public int ProductsQuantity { get; set; } = 0;

        // 🔗 FK → User
        public int UserId { get; set; }
        public User User { get; set; }

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}
