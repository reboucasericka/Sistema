using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Service")]
    public class Service : IEntity
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [StringLength(100)]
        public string? Duration { get; set; }  // Ex: "45 min"

        public bool IsActive { get; set; } = true;

        // Propriedades adicionais para compatibilidade
        public int DurationInMinutes { get; set; } = 60; // Duração em minutos
        public decimal Commission { get; set; } = 0; // Comissão do profissional
        public int ReturnDays { get; set; } = 0; // Dias para retorno

        [StringLength(255)]
        public string? PhotoPath { get; set; }

        // FK
        public int CategoryId { get; set; }
        public Category Category { get; set; }

        // Propriedades de navegação para compatibilidade
        [NotMapped]
        public Category ServiceCategory => Category;

        [NotMapped]
        public ICollection<ProfessionalService> ProfessionalServices => Professionals?.Select(p => new ProfessionalService { Professional = p, Service = this }).ToList() ?? new List<ProfessionalService>();

        // Relacionamento
        public ICollection<Professional>? Professionals { get; set; }
    }
}