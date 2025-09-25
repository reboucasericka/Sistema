using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Esteticas")]
    public class Estetica
    {
        [Key]
        public int EsteticaId { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; }

        [Required, StringLength(200)]
        public string Endereco { get; set; }

        [StringLength(20)]
        public string? Telemovel { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public bool Ativo { get; set; } = true;

        public bool ExportadoExcel { get; set; } = false;
        public bool ExportadoPdf { get; set; } = false;
    }
}