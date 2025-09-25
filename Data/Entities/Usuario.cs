using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Usuarios")]
    public class Usuario : IdentityUser
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nome { get; set; }

        public string? Apelido { get; set; }//opcional

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? Nif { get; set; }

        [Required]
        [MaxLength(200)]
        public string SenhaHash { get; set; }

        // 🔗 FK → Perfil
        public int PerfilId { get; set; }
        public Perfil Perfil { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;

        [StringLength(20)]
        public string? Telemovel { get; set; }

        [StringLength(200)]
        public string? Endereco { get; set; }

        [StringLength(200)]
        public string? Foto { get; set; }

        // 🔗 Relação 1:N (um usuário pode ter vários registros de acesso)
        public ICollection<RegistroAcesso> RegistroAcessos { get; set; }
    }
}