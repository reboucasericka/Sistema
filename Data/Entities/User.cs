using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sistema.Data.Entities
{
    [Table("Users")] // a tabela vai chamar-se "Users" em vez de "AspNetUsers"
    public class User : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool Active { get; set; } = true;

        public Guid? ImageId { get; set; }

        // Se quiseres mais campos personalizados, adiciona aqui
    }
}