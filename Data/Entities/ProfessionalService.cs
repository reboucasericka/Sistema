using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("ProfessionalServices")]
    public class ProfessionalService
    {
        [Key]
        public int ProfessionalServiceId { get; set; }

        // 🔗 FK → Professional
        public int ProfessionalId { get; set; }
        public Professional Professional { get; set; }

        // 🔗 FK → Service
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Commission { get; set; } // Comissao
    }
}