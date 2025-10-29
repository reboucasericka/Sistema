using System.ComponentModel.DataAnnotations;

namespace SistemaAPI.DTOs
{
    public class ClientDto
    {
        public int Id { get; set; }
        public int ClientId => Id;
        
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O sobrenome é obrigatório")]
        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
        
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
        
        [Phone(ErrorMessage = "Telefone inválido")]
        public string? Phone { get; set; }
        
        public string? Address { get; set; }
        
        public string? City { get; set; }
        
        public string? State { get; set; }
        
        public string? ZipCode { get; set; }
        
        public DateTime? BirthDate { get; set; }
        
        public string? Gender { get; set; }
        
        public string? Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Propriedades de navegação para compatibilidade com Views
        public string ClientName => FullName;
        public string ClientEmail => Email;
        public string ClientPhone => Phone ?? string.Empty;
        
        // Propriedades de compatibilidade
        public string Name => FullName;
        public DateTime RegistrationDate => CreatedAt;
    }
}
