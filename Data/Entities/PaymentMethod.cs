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
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        // 🔗 Relationships
        public ICollection<Payable> Payables { get; set; }
        public ICollection<Receivable> Receivables { get; set; }
    }
}