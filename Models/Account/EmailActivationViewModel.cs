using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Account
{
    public class EmailActivationViewModel
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public string Token { get; set; }
        
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
