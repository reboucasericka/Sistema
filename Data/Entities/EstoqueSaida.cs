using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("EstoqueSaidas")]
    public class EstoqueSaida
    {
        [Key]
        public int SaidaId { get; set; }

        // 🔗 FK → Produto
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }

        public int Quantidade { get; set; }

        [Required, StringLength(100)]
        public string Motivo { get; set; }

        // 🔗 FK → Usuario (quem lançou)
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // 🔗 FK → Agendamento (quando saída foi para um serviço)
        public int? AgendamentoId { get; set; }
        public Agendamento? Agendamento { get; set; }

        [Column(TypeName = "date")]
        public DateTime Data { get; set; }

        [StringLength(20)]
        public string TipoMovimentacao { get; set; } = "saida"; // saida, perda, uso_servico

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorUnitario { get; set; }

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}