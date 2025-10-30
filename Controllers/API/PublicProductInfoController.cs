using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Entities;
using SistemaAPI.DTOs;
using SistemaAPI.DTOs.Common;

namespace Sistema.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicProductInfoController : ControllerBase
    {
        private readonly SistemaDbContext _context;

        public PublicProductInfoController(SistemaDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all public product information
        /// </summary>
        /// <returns>List of public product information</returns>
        [HttpGet]
        public async Task<IActionResult> GetPublicProductInfos()
        {
            try
            {
                var products = await _context.PublicProductInfos
                    .AsNoTracking()
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                var productDtos = products.Select(p => new PublicProductInfoDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ApplicationMethod = p.ApplicationMethod,
                    Ingredients = p.Ingredients,
                    Brand = p.Brand,
                    Category = "Cosméticos Profissionais", // Categoria fixa para produtos estéticos
                    IsActive = p.IsActive,
                    ImageId = p.ImageId ?? Guid.Empty,
                    CreatedAt = p.CreatedAt
                }).ToList();

                return Ok(ApiResponse<List<PublicProductInfoDto>>.SuccessResult(productDtos, $"Retrieved {productDtos.Count} products"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<PublicProductInfoDto>>.ErrorResult("Error retrieving public product information", 500, new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get public product information by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Public product information</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPublicProductInfo(int id)
        {
            try
            {
                var product = await _context.PublicProductInfos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null)
                {
                    return NotFound(ApiResponse<PublicProductInfoDto>.ErrorResult("Product information not found", 404));
                }

                var productDto = new PublicProductInfoDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    ApplicationMethod = product.ApplicationMethod,
                    Ingredients = product.Ingredients,
                    Brand = product.Brand,
                    Category = "Cosméticos Profissionais", // Categoria fixa para produtos estéticos
                    IsActive = product.IsActive,
                    ImageId = product.ImageId ?? Guid.Empty,
                    CreatedAt = product.CreatedAt
                };

                return Ok(ApiResponse<PublicProductInfoDto>.SuccessResult(productDto, "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PublicProductInfoDto>.ErrorResult("Error retrieving product information", 500, new List<string> { ex.Message }));
            }
        }
    }
}
