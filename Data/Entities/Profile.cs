using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Profiles")]
    public class Profile
    {
        [Key]
        public int ProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // 🔗 Relação 1:N (um perfil pode ter vários usuários)
        public ICollection<User> Users { get; set; }
    }
}