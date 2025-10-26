using Sistema.Data.Entities;

namespace Sistema.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        string GetUserIdFromToken(string token);
    }
}
