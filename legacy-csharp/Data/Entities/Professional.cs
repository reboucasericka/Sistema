using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Professionals")]
    public class Professional : IEntity
    {
        [Key]
        public int ProfessionalId { get; set; }
        
        

        // 🔗 FK → User
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Nome do profissional

        public Guid ImageId { get; set; } // Foto de perfil (ID da imagem no blob)

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } // Especialidade

        [StringLength(20)]
        public string? Phone { get; set; } // Telefone do profissional

        [StringLength(100)]
        public string? Email { get; set; } // Email do profissional

        [Column(TypeName = "decimal(5,2)")]
        public decimal CommissionPercentage { get; set; } = 0; // Percentual de comissão (0-100)

        [Column(TypeName = "decimal(5,2)")]
        public decimal DefaultCommission { get; set; } = 0; // Comissão padrão (0-100)

        public bool IsActive { get; set; } = true;

        // 🔗 Relacionamentos
        public ICollection<ProfessionalService> ProfessionalServices { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Schedule> Schedules { get; set; }

        // Propriedade calculada para o caminho completo da imagem
        public string ImageFullPath => ImageId == Guid.Empty
            ? "/images/noimage.png"
            : $"/uploads/professionals/{ImageId}.png";
    }
}