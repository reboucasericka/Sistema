using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Get all products with user information
        /// </summary>
        /// <returns>List of products</returns>
        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                var products = _productRepository.GetAllWithUsers();
                return Ok(new { 
                    success = true, 
                    products = products,
                    count = products.Count()
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
