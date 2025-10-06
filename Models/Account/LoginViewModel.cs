using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O usuário ou e-mail é obrigatório")]
        public string? Username { get; set; }


        [Required(ErrorMessage = "A senha é obrigatória")]
        [MinLength(3, ErrorMessage = "A senha deve ter pelo menos 3 caracteres")]
        public string? Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}
