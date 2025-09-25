using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Profissionais")]
    public class Profissional
    {
        [Key]
        public int ProfissionalId { get; set; }

        // 🔗 FK → Usuario
        public int UsuarioId { get; set; }
        [Required]
        [MaxLength(100)]
        public Usuario Usuario { get; set; }

        [Required]
        [MaxLength(100)]
        public string Especialidade { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ComissaoPadrao { get; set; } = 0;

        public bool Ativo { get; set; } = true;

        // 🔗 Relações
        public ICollection<ProfissionalServico> ProfissionalServicos { get; set; }
        public ICollection<Agendamento> Agendamentos { get; set; }
        public ICollection<Horario> Horarios { get; set; }
    }
}