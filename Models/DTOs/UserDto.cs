using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O nome de usuário é obrigatório")]
        [StringLength(50, ErrorMessage = "O nome de usuário deve ter no máximo 50 caracteres")]
        public string UserName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O sobrenome é obrigatório")]
        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
        
        [Phone(ErrorMessage = "Telefone inválido")]
        public string? PhoneNumber { get; set; }
        
        public bool EmailConfirmed { get; set; }
        
        public bool PhoneNumberConfirmed { get; set; }
        
        public bool TwoFactorEnabled { get; set; }
        
        public bool LockoutEnabled { get; set; }
        
        public DateTimeOffset? LockoutEnd { get; set; }
        
        public int AccessFailedCount { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Propriedades para compatibilidade com Views
        public string UserEmail => Email;
        public string UserPhone => PhoneNumber ?? string.Empty;
    }
}
