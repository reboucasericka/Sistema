using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("CashRegisters")]
    public class CashRegister
    {
        [Key]
        public int CashRegisterId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal InitialValue { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FinalValue { get; set; }

        // 🔗 FK → Opening User
        public int OpeningUserId { get; set; }
        public User OpeningUser { get; set; }

        // 🔗 FK → Closing User (optional)
        public int? ClosingUserId { get; set; }
        public User? ClosingUser { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "open";

        public string? Notes { get; set; }

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}
