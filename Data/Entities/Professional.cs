using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Professionals")]
    public class Professional
    {
        [Key]
        public int ProfessionalId { get; set; }

        // 🔗 FK → User
        public string UserId { get; set; }
        public User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Nome do profissional

        [MaxLength(200)]
        public string? PhotoPath { get; set; } // Foto de perfil (caminho da imagem)

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; } // Especialidade

        [StringLength(20)]
        public string? Phone { get; set; } // Telefone do profissional

        [StringLength(100)]
        public string? Email { get; set; } // Email do profissional

        [Column(TypeName = "decimal(10,2)")]
        public decimal DefaultCommission { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // 🔗 Relacionamentos
        public ICollection<ProfessionalService> ProfessionalServices { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
}