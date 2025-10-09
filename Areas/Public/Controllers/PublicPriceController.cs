using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sistema.Data;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicPriceController : Controller
    {
        private readonly IPriceTableRepository _priceTableRepository;

        public PublicPriceController(IPriceTableRepository priceTableRepository)
        {
            _priceTableRepository = priceTableRepository;
        }

        public IActionResult Index()
        {
            var prices = _priceTableRepository.GetAllOrdered().ToList();
            return View(prices);
        }

        public async Task<IActionResult> Details(int id)
        {
            var price = await _priceTableRepository.GetByIdAsync(id);
            if (price == null) return NotFound();

            return View(price);
        }
    }
}