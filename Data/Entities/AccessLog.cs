using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("RegistroAcessos")]
    public class AccessLog
    {
        [Key]
        public int RegistroAcessosId { get; set; }

        // 🔗 FK → Usuario
        public int UsuarioId { get; set; }
        public User Usuario { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataHora { get; set; } = DateTime.Now;

        [Required, StringLength(100)]
        public string Acao { get; set; }

        [StringLength(50)]
        public string? IpAddress { get; set; }
    }
}