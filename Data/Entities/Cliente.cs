using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Clientes")]
    public class Cliente
    {
        [Key]
        public int ClienteId { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telemovel { get; set; }

        [StringLength(200)]
        public string? Endereco { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DataNascimento { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;

        public string? Observacoes { get; set; }

        [StringLength(255)]
        public string? HistoricoAlergias { get; set; }
    }
}