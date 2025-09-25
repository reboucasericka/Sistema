using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Faturacao")]
    public class Faturacao
    {
        [Key]
        public int FaturacaoId { get; set; }

        [Column(TypeName = "date")]
        public DateTime Data { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorTotal { get; set; }

        public int QuantidadeServicos { get; set; } = 0;

        public int QuantidadeProdutos { get; set; } = 0;

        // 🔗 FK → Usuario
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}
