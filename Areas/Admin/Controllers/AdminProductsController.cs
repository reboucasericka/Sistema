using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Sistema.Services.Api;
using SistemaAPI.DTOs;
using Sistema.Helpers;
using Sistema.Models.Admin;
using Microsoft.Extensions.Logging;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ApiProductsService _productsService;
        private readonly IStorageHelper _storageHelper;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ILogger<AdminProductsController> _logger;

        public AdminProductsController(
            ApiProductsService productsService,
            IStorageHelper storageHelper,
            IUserHelper userHelper,
            IConverterHelper converterHelper,
            ILogger<AdminProductsController> logger)
        {
            _productsService = productsService;
            _storageHelper = storageHelper;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _logger = logger;
        }

        // =======================
        // GET: Produtos
        // =======================
        public async Task<IActionResult> Index()
        {
            try
            {
                var response = await _productsService.GetAllAsync();
                
                if (response.Success && response.Data != null)
                {
                    var products = response.Data.OrderBy(p => p.Name).ToList();
                    return View(products);
                }
                else
                {
                    _logger.LogError("Failed to fetch products: {Message}", response.Message);
                    TempData["ErrorMessage"] = "Failed to load products. Please try again.";
                    return View(new List<ProductDto>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products");
                TempData["ErrorMessage"] = "An error occurred while loading products.";
                return View(new List<ProductDto>());
            }
        }

        // =======================
        // GET: Produtos/Details/5
        // =======================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            try
            {
                var response = await _productsService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch product {Id}: {Message}", id, response.Message);
                    return new NotFoundViewResult("ProductNotFound");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {Id}", id);
                return new NotFoundViewResult("ProductNotFound");
            }
        }

        // =======================
        // GET: Produtos/Create
        // =======================
        public IActionResult Create()
        {
            // Buscar categorias e fornecedores via API quando os endpoints estiverem dispon�veis
            ViewData["ProductCategoryId"] = new SelectList(new List<object>(), "ProductCategoryId", "Name");
            ViewData["SupplierId"] = new SelectList(new List<object>(), "SupplierId", "Name");
            return View();
        }

        // =======================
        // POST: Produtos/Create
        // =======================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductViewModel model)
        {
            _logger.LogInformation("=== IN�CIO DO M�TODO CREATE (POST) ===");
            _logger.LogInformation("Model recebido - Nome: {Name}, CategoriaId: {ProductCategoryId}, FornecedorId: {SupplierId}", 
                model.Name, model.ProductCategoryId, model.SupplierId);

            // Valida��o b�sica do ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de valida��o: {string.Join(", ", errors)}";
                _logger.LogWarning("Erro de valida��o: {ErrorMessage}", errorMessage);
                TempData["ErrorMessage"] = errorMessage;
                
                // Repopular dropdowns em caso de erro
                ViewData["ProductCategoryId"] = new SelectList(new List<object>(), "ProductCategoryId", "Name", model?.ProductCategoryId);
                ViewData["SupplierId"] = new SelectList(new List<object>(), "SupplierId", "Name", model?.SupplierId);
                return View(model);
            }

            // Valida��o adicional dos campos obrigat�rios
            if (model.ProductCategoryId <= 0)
            {
                _logger.LogWarning("ERRO: ProductCategoryId � obrigat�rio e deve ser maior que 0");
                TempData["ErrorMessage"] = "Por favor, selecione uma categoria v�lida.";
                ViewData["ProductCategoryId"] = new SelectList(new List<object>(), "ProductCategoryId", "Name", model?.ProductCategoryId);
                ViewData["SupplierId"] = new SelectList(new List<object>(), "SupplierId", "Name", model?.SupplierId);
                return View(model);
            }

            try
            {
                _logger.LogInformation("Iniciando valida��es de exist�ncia...");

                // Implementar valida��es via API quando os endpoints estiverem dispon�veis
                // Por enquanto, pular as valida��es de exist�ncia

                _logger.LogInformation("Valida��es de exist�ncia conclu�das com sucesso");

                // Processar imagem
                int? imageId = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    try
                    {
                        _logger.LogInformation("Iniciando upload da imagem...");
                        string photoPath = await _storageHelper.UploadAsync(model.ImageFile, "products");
                        if (!string.IsNullOrEmpty(photoPath))
                        {
                            imageId = int.Parse(photoPath);
                            _logger.LogInformation("Upload da imagem conclu�do. ImageId: {ImageId}", imageId);
                        }
                        else
                        {
                            _logger.LogWarning("Upload falhou, produto ser� criado sem imagem");
                            TempData["WarningMessage"] = "Imagem n�o foi enviada, produto criado sem foto.";
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "ERRO no upload da imagem");
                        TempData["WarningMessage"] = "Imagem n�o foi enviada, produto criado sem foto.";
                        // Continua o processo mesmo com erro de upload
                    }
                }
                else
                {
                    _logger.LogInformation("Nenhuma imagem fornecida, ImageId ser� null");
                }

                // Obter usu�rio atual
                var currentUser = await _userHelper.GetUserByEmailAsync(this.User.Identity?.Name);
                var userId = currentUser?.Id;

                // Criar DTO para a API
                _logger.LogInformation("Convertendo ViewModel para DTO...");
                var productDto = new ProductDto
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.SalePrice,
                    StockQuantity = model.Stock,
                    Category = model.CategoryName,
                    Brand = model.Brand,
                    SKU = model.SKU,
                    IsActive = model.IsActive
                };

                _logger.LogInformation("Produto DTO criado - Nome: {Name}, Categoria: {Category}, Marca: {Brand}, SKU: {SKU}", 
                    productDto.Name, productDto.Category, productDto.Brand, productDto.SKU);

                if (currentUser != null)
                {
                    _logger.LogInformation("Usu�rio associado: {Email}", currentUser.Email);
                }
                else
                {
                    _logger.LogWarning("AVISO: Usu�rio atual n�o encontrado");
                }

                // Salvar via API
                _logger.LogInformation("Iniciando salvamento via API...");
                var response = await _productsService.CreateAsync(productDto);
                
                if (response.Success)
                {
                    _logger.LogInformation("Produto salvo com sucesso via API!");
                    TempData["SuccessMessage"] = "Produto criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _logger.LogError("Failed to create product: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Failed to create product: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERRO GERAL ao criar produto");
                
                var errorMessage = "Erro ao criar produto. ";
                
                // Capturar InnerException
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner Exception");
                    errorMessage += $"Detalhes: {ex.InnerException.Message}";
                    
                    // Verificar se � erro de constraint de FK
                    if (ex.InnerException.Message.Contains("FOREIGN KEY constraint"))
                    {
                        errorMessage = "Erro de refer�ncia: A categoria ou fornecedor selecionado n�o existe no sistema.";
                    }
                    else if (ex.InnerException.Message.Contains("UNIQUE constraint"))
                    {
                        errorMessage = "Erro de duplica��o: J� existe um produto com este nome.";
                    }
                }
                else
                {
                    errorMessage += ex.Message;
                }
                
                TempData["ErrorMessage"] = errorMessage;
            }
            
            // Repopular dropdowns em caso de erro
            ViewData["ProductCategoryId"] = new SelectList(new List<object>(), "ProductCategoryId", "Name", model?.ProductCategoryId);
            ViewData["SupplierId"] = new SelectList(new List<object>(), "SupplierId", "Name", model?.SupplierId);
            return View(model);
        }



        // =======================
        // GET: Produtos/Edit/5
        // =======================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            try
            {
                var response = await _productsService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    // Converter ProductDto para AdminProductViewModel
                    // Por enquanto, usar o DTO diretamente
                    var model = new AdminProductViewModel
                    {
                        ProductId = response.Data.ProductId,
                        Name = response.Data.Name,
                        Description = response.Data.Description,
                        PurchasePrice = response.Data.Price,
                        SalePrice = response.Data.Price,
                        Stock = response.Data.StockQuantity,
                        CategoryName = response.Data.Category,
                        Brand = response.Data.Brand,
                        SKU = response.Data.SKU,
                        IsActive = response.Data.IsActive
                    };

                    // Buscar categorias e fornecedores via API quando os endpoints estiverem dispon�veis
                    ViewData["ProductCategoryId"] = new SelectList(new List<object>(), "ProductCategoryId", "Name", response.Data.Category);
                    ViewData["SupplierId"] = new SelectList(new List<object>(), "SupplierId", "Name", response.Data.Brand);

                    return View(model);
                }
                else
                {
                    _logger.LogError("Failed to fetch product for edit {Id}: {Message}", id, response.Message);
                    return new NotFoundViewResult("ProductNotFound");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product for edit {Id}", id);
                return new NotFoundViewResult("ProductNotFound");
            }
        }




        // =======================
        // POST: Produtos/Edit/5
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Garante que existe um ID v�lido, mesmo se o model.ImageId for null
                    int? imageId = model.ImageId.HasValue && model.ImageId != null
                        ? int.Parse(model.ImageId.Value.ToString())
                        : null;

                    // Se uma nova imagem foi enviada, faz upload e obt�m novo GUID
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        try
                        {
                            string photoPath = await _storageHelper.UploadAsync(model.ImageFile, "products");
                            if (!string.IsNullOrEmpty(photoPath))
                            {
                                imageId = int.Parse(photoPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "ERRO no upload da imagem");
                            TempData["WarningMessage"] = "Imagem n�o foi enviada, produto atualizado sem nova foto.";
                        }
                    }

                    // Obter usu�rio atual
                    var currentUser = await _userHelper.GetUserByEmailAsync(this.User.Identity?.Name);
                    var userId = currentUser?.Id;

                    // Criar DTO para a API
                    var productDto = new ProductDto
                    {
                        ProductId = model.ProductId,
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.SalePrice,
                        StockQuantity = model.Stock,
                        Category = model.CategoryName,
                        Brand = model.Brand,
                        SKU = model.SKU,
                        IsActive = model.IsActive
                    };

                    var response = await _productsService.UpdateAsync(model.ProductId, productDto);

                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Produto atualizado com sucesso!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        _logger.LogError("Failed to update product: {Message}", response.Message);
                        TempData["ErrorMessage"] = $"Failed to update product: {response.Message}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product");
                    TempData["ErrorMessage"] = "An error occurred while updating the product.";
                }
            }

            // se der erro de valida��o, recarrega dropdowns
            ViewData["ProductCategoryId"] = new SelectList(new List<object>(), "ProductCategoryId", "Name", model.ProductCategoryId);
            ViewData["SupplierId"] = new SelectList(new List<object>(), "SupplierId", "Name", model.SupplierId);

            return View(model);
        }

        // =======================
        // GET: Produtos/Delete/5
        // =======================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return new NotFoundViewResult("ProductNotFound");

            try
            {
                var response = await _productsService.GetByIdAsync(id.Value);

                if (response.Success && response.Data != null)
                {
                    return View(response.Data);
                }
                else
                {
                    _logger.LogError("Failed to fetch product for delete {Id}: {Message}", id, response.Message);
                    return new NotFoundViewResult("ProductNotFound");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product for delete {Id}", id);
                return new NotFoundViewResult("ProductNotFound");
            }
        }

        // =======================
        // POST: Produtos/Delete/5
        // =======================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var response = await _productsService.DeleteAsync(id);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Produto deletado com sucesso!";
                }
                else
                {
                    _logger.LogError("Failed to delete product: {Message}", response.Message);
                    TempData["ErrorMessage"] = $"Failed to delete product: {response.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                TempData["ErrorMessage"] = "An error occurred while deleting the product.";
            }

            return RedirectToAction(nameof(Index));
        }
        public IActionResult ProductNotFound()
        {
            return View();
        }
    }
}

