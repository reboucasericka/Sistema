using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Account
{
    public class ChangeUserViewModel
    {
        [Required]
        [Display(Name = "Nome")]
        public string? FirstName { get; set; }

        [Required]
        [Display(Name = "Apelido")]
        public string? LastName { get; set; }
    }
}
