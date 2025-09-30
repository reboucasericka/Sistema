using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Implementations;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;
using Sistema.Models;
using System.IO;

namespace Sistema.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {


        private readonly IProductRepository _productRepository;
        private readonly IProductCategoryRepository _productCategoryRepository;
        private readonly ISupplierRepository _supplierRepository;        
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        private readonly IConverterHelper _converterHelper;
        

        // Construtor injeta apenas IProdutoRepository
        public ProductsController(
            IProductRepository productRepository,
            IProductCategoryRepository productCategoryRepository,
            ISupplierRepository supplierRepository,
            IUserHelper userHelper, 
            IBlobHelper blobHelper,
            IConverterHelper converterHelper)
        {

            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _supplierRepository = supplierRepository;            
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            _converterHelper = converterHelper;
            
        }

        // =======================
        // GET: Produtos
        // =======================
        public IActionResult Index()
        {           
            // Usar ele em vez do GetAll() simples (para trazer Categoria e Fornecedor).
            var products = _productRepository.GetAllWithIncludes().OrderBy(p => p.Name);  //ordenar por Name
            return View(products);
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
            var products = await _productRepository.GetByIdAsync(id.Value);
            if (products == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            return View(products);
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
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;
                
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");
                }



                var products = _converterHelper.ToProduct(model, imageId, true);
                products.User = await _userHelper.GetUserByEmailAsync(this.User.Identity.Name);
                await _productRepository.CreateAsync(products); // ✅ agora usa método genérico
                return RedirectToAction(nameof(Index));
            }
            // ⚠️ repetir SelectList caso o ModelState seja inválido
            ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(), "ProductCategoryId", "Name", model.ProductCategoryId);
            ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(), "SupplierId", "Name", model.SupplierId);
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
            var products = await _productRepository.GetByIdAsync(id.Value);
            if (products == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            // Converte Product → ProductViewModel
            var model = _converterHelper.ToProductViewModel(products);
            // ANTES DE RETORNAR A VIEW, PRECISAMOS CONVERTER O PRODUCT PARA O PRODUCTVIEWMODEL
            // Popula dropdowns
            ViewData["ProductCategoryId"] = new SelectList(_productCategoryRepository.GetAll(),"ProductCategoryId","Name",products.ProductCategoryId);
            ViewData["SupplierId"] = new SelectList(_supplierRepository.GetAll(),"SupplierId","Name",products.SupplierId);

            return View(model);
        }




        // =======================
        // POST: Produtos/Edit/5
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    Guid imageId= model.ImageId; // mantém a foto existente

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {

                        imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "products");

                    }
                    var products = _converterHelper.ToProduct(model, imageId, false);

                    //Atribuir o usuário logado
                    products.User = await _userHelper.GetUserByEmailAsync(User.Identity.Name);
                    await _productRepository.UpdateAsync(products); // ✅ genérico cobre Update
                }
                catch
                {
                    if (!await _productRepository.ExistsAsync(model.ProductId))
                        return NotFound();
                    else
                        throw;
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

            var products = await _productRepository.GetByIdAsync(id.Value);
            if (products == null) return new NotFoundViewResult("ProductNotFound");

            return View(products);
        }

        // =======================
        // POST: Produtos/Delete/5
        // =======================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var products = await _productRepository.GetByIdAsync(id);
            await _productRepository.DeleteAsync(products);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult ProductNotFound()
        {
            return View();
        }
    }
}
