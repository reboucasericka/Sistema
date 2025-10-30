using Microsoft.AspNetCore.Mvc;
using Sistema.Services.Api;
using SistemaAPI.DTOs;

namespace Sistema.Areas.Public.Controllers
{
    [Area("Public")]
    public class PublicProductInfoController : Controller
    {
        private readonly IApiClientService _api;
        
        public PublicProductInfoController(IApiClientService api) => _api = api;

        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _api.GetAsync<List<PublicProductInfoDto>>("api/public-product-info");
                return View(response.Data ?? new List<PublicProductInfoDto>());
            }
            catch
            {
                return View(new List<PublicProductInfoDto>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var response = await _api.GetAsync<PublicProductInfoDto>($"api/public-product-info/{id}");
                if (response.Data == null)
                {
                    TempData["Error"] = "Produto não encontrado.";
                    return RedirectToAction("Index");
                }
                return View(response.Data);
            }
            catch
            {
                TempData["Error"] = "Erro ao carregar produto.";
                return RedirectToAction("Index");
            }
        }
    }
}
