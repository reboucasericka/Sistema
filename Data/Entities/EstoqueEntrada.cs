using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("EstoqueEntradas")]
    public class EstoqueEntrada
    {
        [Key]
        public int EntradaId { get; set; }

        // 🔗 FK → Produto
        public int ProdutoId { get; set; }
        public Product Produto { get; set; }

        public int Quantidade { get; set; }

        [Required, StringLength(100)]
        public string Motivo { get; set; }

        // 🔗 FK → Usuario (quem lançou)
        public int UsuarioId { get; set; }
        public User Usuario { get; set; }

        // 🔗 FK → Fornecedor (opcional)
        public int? FornecedorId { get; set; }
        public Supplier? Fornecedor { get; set; }

        [Column(TypeName = "date")]
        public DateTime Data { get; set; }

        [StringLength(20)]
        public string TipoMovimentacao { get; set; } = "entrada";

        [StringLength(50)]
        public string? Lote { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DataValidade { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ValorUnitario { get; set; }

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}