using Microsoft.Graph.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Services")]
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        // 🔗 FK → ServiceCategory
        public int ServiceCategoryId { get; set; }
        public Service ServiceCategory { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(200)]
        public string? Photo { get; set; }

        public int? ReturnDays { get; set; }

        public bool Active { get; set; } = true;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Commission { get; set; }
    }
}