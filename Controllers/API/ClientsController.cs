using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sistema.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Sistema.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly SistemaDbContext _context;

        public ClientsController(SistemaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all active clients
        /// </summary>
        /// <returns>List of active clients</returns>
        [HttpGet]
        public async Task<IActionResult> GetClients()
        {
            try
            {
                var clients = await _context.Users
                    .Where(u => u.Active == true)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.CreatedAt,
                        u.Active
                    })
                    .ToListAsync();

                return Ok(clients);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar clientes", error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific client by ID
        /// </summary>
        /// <param name="id">Client ID</param>
        /// <returns>Client information</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClient(string id)
        {
            try
            {
                var client = await _context.Users
                    .Where(u => u.Id == id && u.Active == true)
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.PhoneNumber,
                        u.CreatedAt,
                        u.Active
                    })
                    .FirstOrDefaultAsync();

                if (client == null)
                {
                    return NotFound(new { message = "Cliente não encontrado" });
                }

                return Ok(client);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao buscar cliente", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new client
        /// </summary>
        /// <param name="request">Client information</param>
        /// <returns>Created client</returns>
        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar se já existe um usuário com este email
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Já existe um usuário com este email" });
                }

                var newUser = new Sistema.Data.Entities.User
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    Active = true,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetClient), new { id = newUser.Id }, new
                {
                    newUser.Id,
                    newUser.FirstName,
                    newUser.LastName,
                    newUser.Email,
                    newUser.PhoneNumber,
                    newUser.CreatedAt,
                    newUser.Active
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao criar cliente", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(string id, [FromBody] UpdateClientRequest request)
        {
            try
            {
                var client = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Active == true);
                if (client == null)
                {
                    return NotFound(new { message = "Cliente não encontrado" });
                }

                client.FirstName = request.FirstName ?? client.FirstName;
                client.LastName = request.LastName ?? client.LastName;
                client.PhoneNumber = request.PhoneNumber ?? client.PhoneNumber;
                client.Active = request.IsActive ?? client.Active;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    client.Id,
                    client.FirstName,
                    client.LastName,
                    client.Email,
                    client.PhoneNumber,
                    client.CreatedAt,
                    client.Active
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao atualizar cliente", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(string id)
        {
            try
            {
                var client = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Active == true);
                if (client == null)
                {
                    return NotFound(new { message = "Cliente não encontrado" });
                }

                // Verificar se há agendamentos ou vendas associadas
                var hasAppointments = await _context.Appointments.AnyAsync(a => a.CustomerId.ToString() == id);
                var hasBilling = await _context.Billings.AnyAsync(b => b.UserId.ToString() == id);

                if (hasAppointments || hasBilling)
                {
                    // Desativar em vez de deletar
                    client.Active = false;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Cliente desativado com sucesso" });
                }

                _context.Users.Remove(client);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cliente removido com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao remover cliente", error = ex.Message });
            }
        }
    }

    public class CreateClientRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(50, ErrorMessage = "Nome deve ter no máximo 50 caracteres")]
        public string FirstName { get; set; }

        [StringLength(50, ErrorMessage = "Sobrenome deve ter no máximo 50 caracteres")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Telefone inválido")]
        public string PhoneNumber { get; set; }
    }

    public class UpdateClientRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
    }
}
