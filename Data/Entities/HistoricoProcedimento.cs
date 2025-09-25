using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("HistoricoProcedimentos")]
    public class HistoricoProcedimento
    {
        [Key]
        public int ProcedimentoId { get; set; }

        // 🔗 FK → Cliente
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        // 🔗 FK → Servico
        public int ServicoId { get; set; }
        public Servico Servico { get; set; }

        // 🔗 FK → Profissional
        public int ProfissionalId { get; set; }
        public Profissional Profissional { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataProcedimento { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? MaterialUsado { get; set; } // Ex.: Fios 12mm, Curvatura D

        public string? ObservacoesTecnicas { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }
    }
}