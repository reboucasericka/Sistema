using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;
using Sistema.Models.Admin;

namespace Sistema.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {


        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly ISupplierRepository _supplierRepository;        
        private readonly IUserHelper _userHelper;
        private readonly IStorageHelper _storageHelper;        
        private readonly IConverterHelper _converterHelper;
        

        // Construtor injeta apenas IProdutoRepository
        public AdminProductsController(
            IProductRepository productRepository,
            IProductCategoryRepository productCategoryRepository,
            ISupplierRepository supplierRepository,
            IUserHelper userHelper, 
            IStorageHelper storageHelper,           
            IConverterHelper converterHelper)
        {

            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _supplierRepository = supplierRepository;            
            _userHelper = userHelper;
            _storageHelper = storageHelper;            
            _converterHelper = converterHelper;
            
        }

        // =======================
        // GET: Produtos
        // =======================
        public IActionResult Index()
        {           
            // Usar ele em vez do GetAll() simples (para trazer Categoria e Fornecedor).
            var product = _productRepository.GetAllWithIncludes().OrderBy(p => p.Name);  //ordenar por Name
            return View(product);
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
            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            return View(product);
        }

        // =======================
        // GET: Produtos/Create
        // =======================
        public IActionResult Create()
        {
            // ⚠️ Atenção: estas chamadas só vão funcionar se IProdutoRepository expuser
            // métodos para categorias e fornecedores. Se não, criamos ICategoriaProdutoRepository / IFornecedorRepository separados.
            ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(), "ProductCategoryId", "Name");
            ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(), "SupplierId", "Name");
            return View();
        }

        // =======================
        // POST: Produtos/Create
        // =======================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductViewModel model)
        {
            Console.WriteLine("=== INÍCIO DO MÉTODO CREATE (POST) ===");
            Console.WriteLine($"Model recebido - Nome: {model.Name}, CategoriaId: {model.ProductCategoryId}, FornecedorId: {model.SupplierId}");

            // Validação básica do ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                var errorMessage = $"Erros de validação: {string.Join(", ", errors)}";
                Console.WriteLine($"Erro de validação: {errorMessage}");
                TempData["ErrorMessage"] = errorMessage;
                
                // Repopular dropdowns em caso de erro
                ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(), "ProductCategoryId", "Name", model?.ProductCategoryId);
                ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(), "SupplierId", "Name", model?.SupplierId);
                return View(model);
            }

            // Validação adicional dos campos obrigatórios
            if (model.ProductCategoryId <= 0)
            {
                Console.WriteLine("ERRO: ProductCategoryId é obrigatório e deve ser maior que 0");
                TempData["ErrorMessage"] = "Por favor, selecione uma categoria válida.";
                ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(), "ProductCategoryId", "Name", model?.ProductCategoryId);
                ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(), "SupplierId", "Name", model?.SupplierId);
                return View(model);
            }

            try
            {
                Console.WriteLine("Iniciando validações de existência...");

                // Verificar se a categoria existe
                var categoryExists = await _productCategoryRepository.ExistsAsync(model.ProductCategoryId);
                if (!categoryExists)
                {
                    Console.WriteLine($"ERRO: Categoria com ID {model.ProductCategoryId} não existe");
                    TempData["ErrorMessage"] = "A categoria selecionada não existe. Por favor, selecione uma categoria válida.";
                    ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(), "ProductCategoryId", "Name", model?.ProductCategoryId);
                    ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(), "SupplierId", "Name", model?.SupplierId);
                    return View(model);
                }

                // Verificar se o fornecedor existe (se fornecido)
                if (model.SupplierId.HasValue && model.SupplierId.Value > 0)
                {
                    var supplierExists = await _supplierRepository.ExistsAsync(model.SupplierId.Value);
                    if (!supplierExists)
                    {
                        Console.WriteLine($"ERRO: Fornecedor com ID {model.SupplierId.Value} não existe");
                        TempData["ErrorMessage"] = "O fornecedor selecionado não existe. Por favor, selecione um fornecedor válido ou deixe em branco.";
                        ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(), "ProductCategoryId", "Name", model?.ProductCategoryId);
                        ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(), "SupplierId", "Name", model?.SupplierId);
                        return View(model);
                    }
                }

                Console.WriteLine("Validações de existência concluídas com sucesso");

                // Processar imagem
                Guid? imageId = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    try
                    {
                        Console.WriteLine("Iniciando upload da imagem...");
                        string photoPath = await _storageHelper.UploadAsync(model.ImageFile, "products");
                        if (!string.IsNullOrEmpty(photoPath))
                        {
                            imageId = Guid.Parse(photoPath);
                            Console.WriteLine($"Upload da imagem concluído. ImageId: {imageId}");
                        }
                        else
                        {
                            Console.WriteLine("Upload falhou, produto será criado sem imagem");
                            TempData["WarningMessage"] = "Imagem não foi enviada, produto criado sem foto.";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERRO no upload da imagem: {ex.Message}");
                        TempData["WarningMessage"] = "Imagem não foi enviada, produto criado sem foto.";
                        // Continua o processo mesmo com erro de upload
                    }
                }
                else
                {
                    Console.WriteLine("Nenhuma imagem fornecida, ImageId será null");
                }

                // Obter usuário atual
                var currentUser = await _userHelper.GetUserByEmailAsync(this.User.Identity?.Name);
                var userId = currentUser?.Id;

                // Converter ViewModel para Entity
                Console.WriteLine("Convertendo ViewModel para Entity...");
                var product = _converterHelper.ToProduct(model, imageId ?? Guid.Empty, true, userId);
                
                // Garantir que campos obrigatórios estejam corretos
                product.ProductCategoryId = model.ProductCategoryId;
                product.SupplierId = model.SupplierId > 0 ? model.SupplierId : null;
                product.ImageId = imageId; // Pode ser null
                product.IsActive = model.IsActive;

                Console.WriteLine($"Produto convertido - Nome: {product.Name}, CategoriaId: {product.ProductCategoryId}, FornecedorId: {product.SupplierId}, ImageId: {product.ImageId}, UserId: {product.UserId}");

                if (currentUser != null)
                {
                    Console.WriteLine($"Usuário associado: {currentUser.Email}");
                }
                else
                {
                    Console.WriteLine("AVISO: Usuário atual não encontrado");
                }

                // Salvar no banco
                Console.WriteLine("Iniciando salvamento no banco de dados...");
                await _productRepository.CreateAsync(product);
                Console.WriteLine("Produto salvo com sucesso no banco de dados!");
                
                TempData["SuccessMessage"] = "Produto criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERRO GERAL: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                var errorMessage = "Erro ao criar produto. ";
                
                // Capturar InnerException do Entity Framework
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    errorMessage += $"Detalhes: {ex.InnerException.Message}";
                    
                    // Verificar se é erro de constraint de FK
                    if (ex.InnerException.Message.Contains("FOREIGN KEY constraint"))
                    {
                        errorMessage = "Erro de referência: A categoria ou fornecedor selecionado não existe no sistema.";
                    }
                    else if (ex.InnerException.Message.Contains("UNIQUE constraint"))
                    {
                        errorMessage = "Erro de duplicação: Já existe um produto com este nome.";
                    }
                }
                else
                {
                    errorMessage += ex.Message;
                }
                
                TempData["ErrorMessage"] = errorMessage;
            }
            
            // Repopular dropdowns em caso de erro
            ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(), "ProductCategoryId", "Name", model?.ProductCategoryId);
            ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(), "SupplierId", "Name", model?.SupplierId);
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
            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            // Converte Product → ProductViewModel
            var model = _converterHelper.ToProductViewModel(product);
            // ANTES DE RETORNAR A VIEW, PRECISAMOS CONVERTER O PRODUCT PARA O PRODUCTVIEWMODEL
            // Popula dropdowns
            ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(),"ProductCategoryId","Name",product.ProductCategoryId);
            ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(),"SupplierId","Name",product.SupplierId);

            return View(model);
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

                    // Garante que existe um GUID válido, mesmo se o model.ImageId for null
                    Guid imageId = model.ImageId.HasValue && model.ImageId != Guid.Empty
                        ? model.ImageId.Value
                        : Guid.NewGuid();
                    // Se uma nova imagem foi enviada, faz upload e obtém novo GUID

                    if (model.ImageFile != null && model.ImageFile.Length > 0) // Check if the ImageFile is not null
                    {
                        try
                        {
                            string photoPath = await _storageHelper.UploadAsync(model.ImageFile, "products");
                            if (!string.IsNullOrEmpty(photoPath))
                            {
                                imageId = Guid.Parse(photoPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ERRO no upload da imagem: {ex.Message}");
                            TempData["WarningMessage"] = "Imagem não foi enviada, produto atualizado sem nova foto.";
                        }
                    }
                    // Converte e atualiza
                    var product = _converterHelper.ToProduct(model, imageId, false);                    
                    product.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name); //Atribuir o usuário logado

                    await _productRepository.UpdateAsync(product); //  genérico cobre Update
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _productRepository.ExistsAsync(model.ProductId))
                    {
                        return new NotFoundViewResult("ProductNotFound");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // se der erro de validação, recarrega dropdowns
            ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(),"ProductCategoryId","Name",model.ProductCategoryId);
            ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(),"SupplierId","Name",model.SupplierId);

            return View(model);
        }

        // =======================
        // GET: Produtos/Delete/5
        // =======================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return new NotFoundViewResult("ProductNotFound");

            var product = await _productRepository.GetByIdAsync(id.Value);
            if (product == null) return new NotFoundViewResult("ProductNotFound");

            return View(product);
        }

        // =======================
        // POST: Produtos/Delete/5
        // =======================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            await _productRepository.DeleteAsync(product);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult ProductNotFound()
        {
            return View();
        }
    }
}
