using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("EstoqueMovimentacoes")]
    public class EstoqueMovimentacao
    {
        [Key]
        public int MovimentacaoId { get; set; }

        // 🔗 FK → Produto
        public int ProdutoId { get; set; }
        public Produto Produto { get; set; }

        public int Quantidade { get; set; }

        [Required, StringLength(10)]
        public string TipoMovimentacao { get; set; } // entrada, saida, ajuste, perda

        [Required, StringLength(100)]
        public string Motivo { get; set; }

        // 🔗 FK → Usuario
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        // 🔗 FK → Fornecedor (opcional)
        public int? FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataMovimentacao { get; set; } = DateTime.Now;

        [Column(TypeName = "date")]
        public DateTime? DataValidade { get; set; }

        [StringLength(50)]
        public string? Lote { get; set; }
    }
}