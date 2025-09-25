using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Perfis")]
    public class Perfil
    {
        [Key]
        public int PerfilId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        // 🔗 Relação 1:N (um perfil pode ter vários usuários)
        public ICollection<Usuario> Usuarios { get; set; }
    }
}