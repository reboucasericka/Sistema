using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("PaymentMethods")]
    public class PaymentMethod
    {
        [Key]
        public int PaymentMethodId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // 🔗 Relationships
        public ICollection<Payable> Payables { get; set; } = new List<Payable>();
        public ICollection<Receivable> Receivables { get; set; } = new List<Receivable>();
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}