using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : Controller
    {
        private readonly IProductRepository _produtoRepository;

        public ProdutosController(IProductRepository produtoRepository)
        {
            _produtoRepository = produtoRepository;
        }

        public IActionResult GetProducts()
        {

            return Ok(_produtoRepository.GetAllWithUsers());
        }
    }
}
