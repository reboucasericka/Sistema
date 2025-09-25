using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("FaturacaoDetalhes")]
    public class FaturacaoDetalhe
    {
        [Key]
        public int FaturacaoDetalheId { get; set; }

        // 🔗 FK → Faturacao
        public int FaturacaoId { get; set; }
        public Faturacao Faturacao { get; set; }

        [Required, StringLength(20)]
        public string TipoItem { get; set; } // Produto ou Servico

        // 🔗 FK → Produto (opcional)
        public int? ProdutoId { get; set; }
        public Produto? Produto { get; set; }

        // 🔗 FK → Servico (opcional)
        public int? ServicoId { get; set; }
        public Servico? Servico { get; set; }

        public int Quantidade { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorUnitario { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorTotal { get; set; }
    }
}