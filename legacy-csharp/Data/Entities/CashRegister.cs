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
        public string UserIdAbertura { get; set; }
        public User UserAbertura { get; set; }

        // 🔗 FK → Closing User (optional)
        public string? UserIdFechamento { get; set; }
        public User? UserFechamento { get; set; }

        public bool IsClosed { get; set; } = false;

        [StringLength(20)]
        public string Status { get; set; } = "Open"; // Open, Closed

        public string? Notes { get; set; }

        // Navigation properties for user relationships
        public User OpeningUser => UserAbertura;
        public User? ClosingUser => UserFechamento;
        public string OpeningUserId 
        { 
            get => UserIdAbertura; 
            set => UserIdAbertura = value; 
        }

        // Navigation Properties
        public ICollection<CashMovement> CashMovements { get; set; } = new List<CashMovement>();

        public bool ExportedToExcel { get; set; } = false;
        public bool ExportedToPdf { get; set; } = false;
    }
}
