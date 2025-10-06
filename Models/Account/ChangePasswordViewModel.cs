using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Account
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "A senha atual é obrigatória")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(100, ErrorMessage = "A senha deve ter pelo menos {2} caracteres", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "A confirmação da senha não confere")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}