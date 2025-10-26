using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Entities;
using Sistema.Services;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IJwtService _jwtService;

        public AuthController(UserManager<User> userManager, IJwtService jwtService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                {
                    return Unauthorized(new { message = "Credenciais inválidas" });
                }

                var token = _jwtService.GenerateToken(user);
                return Ok(new { 
                    token = token,
                    user = new {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenDto dto)
        {
            try
            {
                var isValid = _jwtService.ValidateToken(dto.Token);
                if (!isValid)
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                return Ok(new { valid = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno do servidor", error = ex.Message });
            }
        }
    }

    public class LoginDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MinLength(4, ErrorMessage = "Senha deve ter pelo menos 4 caracteres")]
        public string Password { get; set; }
    }

    public class ValidateTokenDto
    {
        [Required(ErrorMessage = "Token é obrigatório")]
        public string Token { get; set; }
    }
}
