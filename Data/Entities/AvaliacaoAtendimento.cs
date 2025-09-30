using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("AvaliacoesAtendimento")]
    public class AvaliacaoAtendimento
    {
        [Key]
        public int AvaliacaoId { get; set; }

        // 🔗 FK → Cliente
        public int ClienteId { get; set; }
        public Client Cliente { get; set; }

        // 🔗 FK → Servico
        public int ServicoId { get; set; }
        public Service Servico { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataAvaliacao { get; set; } = DateTime.Now;

        [Range(1, 5)]
        public int Satisfacao { get; set; } // 1 a 5 estrelas

        public string? Comentarios { get; set; }
    }
}