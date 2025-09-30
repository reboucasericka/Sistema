using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Receber")]
    public class Receber
    {
        [Key]
        public int ReceberId { get; set; }

        [StringLength(100)]
        public string? Descricao { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        [Column(TypeName = "date")]
        public DateTime DataLanc { get; set; }

        [Column(TypeName = "date")]
        public DateTime DataVenc { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DataPgto { get; set; }

        // 🔗 FK → Usuario que lançou
        public int UsuarioLanc { get; set; }
        public User UsuarioLancador { get; set; }

        // 🔗 FK → Usuario que deu baixa
        public int? UsuarioBaixa { get; set; }
        public User? UsuarioBaixador { get; set; }

        [StringLength(200)]
        public string? Foto { get; set; }

        public int? PessoaId { get; set; }

        public bool Pago { get; set; } = false;

        // 🔗 FK → Produto (opcional)
        public int? ProdutoId { get; set; }
        public Product? Produto { get; set; }

        public int? Quantidade { get; set; }

        // 🔗 FK → FormaPagamento
        public int? FormaPagamentoId { get; set; }
        public FormaPagamento? FormaPagamento { get; set; }

        public int Parcela { get; set; } = 1;
        public int TotalParcelas { get; set; } = 1;

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}