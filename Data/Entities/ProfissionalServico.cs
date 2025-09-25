using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("ProfissionalServicos")]
    public class ProfissionalServico
    {
        [Key]
        public int ProfissionalServicoId { get; set; }

        // 🔗 FK → Profissional
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }

        // 🔗 FK → Servico
        public int ServicoId { get; set; }
        public Servico Servico { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Comissao { get; set; }
    }
}