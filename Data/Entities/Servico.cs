using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Servicos")]
    public class Servico
    {
        [Key]
        public int ServicoId { get; set; }

        [Required, StringLength(150)]
        public string Nome { get; set; }

        // 🔗 FK → CategoriaServico
        public int CategoriaServicoId { get; set; }
        public CategoriaServico CategoriaServico { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        [StringLength(200)]
        public string? Foto { get; set; }

        public int? DiasRetorno { get; set; }

        public bool Ativo { get; set; } = true;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Comissao { get; set; }
    }
}