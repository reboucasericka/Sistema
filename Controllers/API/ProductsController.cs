using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;

namespace Sistema.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly SistemaDbContext _context;

        public ProductsController(SistemaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all products with user information
        /// </summary>
        /// <returns>List of products</returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            try
            {
                var products = await _context.Products
                    .AsNoTracking()
                    .Include(p => p.ProductCategory)
                    .Include(p => p.Supplier)
                    .ToListAsync();
                
                return Ok(new { 
                    success = true, 
                    products = products,
                    count = products.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error retrieving products", 
                    error = ex.Message 
                });
            }
        }
    }
}
