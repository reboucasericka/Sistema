using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("StockExits")]
    public class StockExit
    {
        [Key]
        public int ExitId { get; set; }

        // 🔗 FK → Product
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        [Required, StringLength(100)]
        public string Reason { get; set; }

        // 🔗 FK → User (who created)
        public string UserId { get; set; }
        public User User { get; set; }

        // 🔗 FK → Appointment (when output was for a service)
        public int? AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [StringLength(20)]
        public string MovementType { get; set; } = "output"; // output, loss, service_use

        [Column(TypeName = "decimal(10,2)")]
        public decimal? UnitValue { get; set; }

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}