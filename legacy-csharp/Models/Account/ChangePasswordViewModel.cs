using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Account
{
    public class ChangePasswordViewModel
    {
        [Required]
        [Display(Name = " Atual Password")]
        public string? CurrentPassword { get; set; }

        [Required]
        [Display(Name = "Nova Password")]
        public string? NewPassword { get; set; }

        [Required]
        [Compare("Nova Password")]
        public string? ConfirmPassword { get; set; } 
    }
}