using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O campo usuário é obrigatório")]
        [Display(Name = "E-mail ou Username")]
        [StringLength(100, ErrorMessage = "O usuário deve ter no máximo 100 caracteres")]
        public string Username { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "A senha deve ter entre 5 e 100 caracteres")]
        public string Password { get; set; }

        [Display(Name = "Lembrar de mim")]
        public bool RememberMe { get; set; }
    }
}
