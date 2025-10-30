namespace SistemaAPI.DTOs
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int? ProfessionalId { get; set; }
        public string? UserName { get; set; }
    }
}
