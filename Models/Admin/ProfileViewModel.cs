using System.ComponentModel.DataAnnotations;

namespace Sistema.Models.Admin
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }
}
