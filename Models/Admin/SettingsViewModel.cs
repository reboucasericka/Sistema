using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Admin
{
    public class SettingsViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O sobrenome é obrigatório")]
        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Número de telefone inválido")]
        public string? PhoneNumber { get; set; }
    }
}
