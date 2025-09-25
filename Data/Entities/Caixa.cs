using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Caixa")]
    public class Caixa
    {
        [Key]
        public int CaixaId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Data { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorInicial { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorFinal { get; set; }

        // 🔗 FK → Usuario abertura
        public int UsuarioAbertura { get; set; }
        public Usuario UsuarioAberturaRef { get; set; }

        // 🔗 FK → Usuario fechamento (opcional)
        public int? UsuarioFechamento { get; set; }
        public Usuario? UsuarioFechamentoRef { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "aberto";

        public string? Observacoes { get; set; }

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}
