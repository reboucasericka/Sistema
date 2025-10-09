using System.ComponentModel.DataAnnotations;
using Sistema.Attributes;

namespace Sistema.Models.Account
{
    public class RegisterNewUserViewModel
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        [Display(Name = "Nome")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "O sobrenome é obrigatório")]
        [Display(Name = "Sobrenome")]
        [StringLength(100, ErrorMessage = "O sobrenome deve ter no máximo 100 caracteres")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "Digite um e-mail válido")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "E-mail")]
        public string? Email { get; set; }

        // >>>> APENAS REQUIRED, SEM QUALQUER FORMATO <<<<
        [Required(ErrorMessage = "O telemóvel é obrigatório")]
        [Display(Name = "Telemóvel")]
        [StringLength(20, ErrorMessage = "O telemóvel deve ter no máximo 20 caracteres")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "O username é obrigatório")]
        [Display(Name = "Username")]
        [StringLength(50, ErrorMessage = "O username deve ter no máximo 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "O username deve conter apenas letras, números e underscore")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
        [Display(Name = "Senha")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "A confirmação de senha é obrigatória")]
        [Compare("Password", ErrorMessage = "As senhas não coincidem")]
        [Display(Name = "Confirmar Senha")]
        public string? Confirm { get; set; }
    }
}