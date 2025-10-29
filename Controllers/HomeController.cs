using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sistema.Models.Account;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using System.Diagnostics;

namespace Sistema.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiServicesService _servicesService;
        private readonly ApiProductsService _productsService;

        public HomeController(
            ILogger<HomeController> logger, 
            ApiServicesService servicesService,
            ApiProductsService productsService)
        {
            _logger = logger;
            _servicesService = servicesService;
            _productsService = productsService;
        }

        public async Task<IActionResult> Index()
        {
            // Busca serviços em destaque (apenas se autenticado)
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    var servicesResponse = await _servicesService.GetAllAsync();
                    var productsResponse = await _productsService.GetAllAsync();

                    var featuredServices = servicesResponse.Success ? 
                        servicesResponse.Data?.Take(6).ToList() ?? new List<ServiceDto>() : 
                        new List<ServiceDto>();

                    var featuredProducts = productsResponse.Success ? 
                        productsResponse.Data?.Where(p => p.IsActive).Take(6).ToList() ?? new List<ProductDto>() : 
                        new List<ProductDto>();

                    ViewBag.FeaturedServices = featuredServices;
                    ViewBag.FeaturedProducts = featuredProducts;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao carregar dados da API");
                    ViewBag.FeaturedServices = new List<ServiceDto>();
                    ViewBag.FeaturedProducts = new List<ProductDto>();
                }
            }

            return View();
        }

        //  Nova página Store (vitrine de produtos)
        [Authorize]
        public async Task<IActionResult> Store()
        {
            try
            {
                var productsResponse = await _productsService.GetAllAsync();
                var products = productsResponse.Success ? 
                    productsResponse.Data?.Where(p => p.IsActive).ToList() ?? new List<ProductDto>() : 
                    new List<ProductDto>();

                return View(products); // procura Views/Home/Store
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar produtos da API");
                return View(new List<ProductDto>());
            }
        }

        [Authorize]
        public async Task<IActionResult> Price()
        {
            try
            {
                // Implementar serviço de API para PriceTables
                // Buscar serviços via API para criar tabela de preços
                var servicesResponse = await _servicesService.GetAllAsync();
                var precos = new List<object>();
                
                if (servicesResponse.IsSuccess && servicesResponse.Data != null)
                {
                    precos = servicesResponse.Data
                        .Where(s => s.IsActive)
                        .Select(s => new
                        {
                            ServiceName = s.Name,
                            Price = s.Price,
                            Description = s.Description,
                            Duration = s.Duration
                        })
                        .Cast<object>()
                        .ToList();
                }
                
                return View(precos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar preços da API");
                return View(new List<object>());
            }
        }

        [Authorize]
        public IActionResult Admin()
        {
            // Simular dados para o painel administrativo
            ViewBag.TotalClients = 150;
            ViewBag.TotalAppointments = 12;
            ViewBag.TotalProducts = 45;
            ViewBag.TotalRevenue = "€ 15.420";
            ViewBag.ActiveClients = 120;
            ViewBag.TotalServices = 8;
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [Route("error/404")]
        public IActionResult Erro404()
        {
            return View();
        }

        // GET: Public About
        public IActionResult About()
        {
            return View();
        }

        // GET: Public Contact
        public IActionResult Contact()
        {
            return View();
        }

        // GET: Test DataTables
        public IActionResult TestDataTables()
        {
            return View();
        }
    }
}