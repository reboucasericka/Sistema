using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;
using Sistema.Helpers;
using Sistema.Models;
using System.IO;

namespace Sistema.Controllers
{
    public class ProdutosController : Controller
    {

        private readonly IProdutoRepository _produtoRepository;
        private readonly ICategoriaProdutoRepository _categoriaProdutoRepository;
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IUsuarioHelper _usuarioHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        // Construtor injeta apenas IProdutoRepository
        public ProdutosController(IProdutoRepository produtoRepository,
            ICategoriaProdutoRepository categoriaProdutoRepository,
            IFornecedorRepository fornecedorRepository,
            IUsuarioHelper usuarioHelper, IImageHelper imageHelper,
            IConverterHelper converterHelper)
        {
           _produtoRepository = produtoRepository;
           _categoriaProdutoRepository = categoriaProdutoRepository;
           _fornecedorRepository = fornecedorRepository;
           _usuarioHelper = usuarioHelper;
           _imageHelper = imageHelper;
           _converterHelper = converterHelper;
        }

        // =======================
        // GET: Produtos
        // =======================
        public IActionResult Index()
        {

            // ⚠️ No ProdutoRepository já tens GetAllWithIncludes()
            // Usar ele em vez do GetAll() simples (para trazer Categoria e Fornecedor).
            var produtos = _produtoRepository.GetAllWithIncludes().OrderBy(p => p.Nome);  //ordenar por Nome
            return View(produtos);
        }

        // =======================
        // GET: Produtos/Details/5
        // =======================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // ⚠️ GetByIdAsync() retorna sem relacionamentos
            // podes criar um GetByIdWithIncludesAsync() no repositório se precisares
            var produto = await _produtoRepository.GetByIdAsync(id.Value);
            if (produto == null) return NotFound();
            return View(produto);
        }

        // =======================
        // GET: Produtos/Create
        // =======================
        public IActionResult Create()
        {
            // ⚠️ Atenção: estas chamadas só vão funcionar se IProdutoRepository expuser
            // métodos para categorias e fornecedores. Se não, criamos ICategoriaProdutoRepository / IFornecedorRepository separados.
            ViewData["CategoriaProdutoId"] = new SelectList(_categoriaProdutoRepository.GetAll(), "CategoriaProdutoId", "Nome");
            ViewData["FornecedorId"] = new SelectList(_fornecedorRepository.GetAll(), "FornecedorId", "Nome");
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
                var path = string.Empty;
                if (model.ImageProductFile != null && model.ImageProductFile.Length > 0)
                {
                    path = await _imageHelper.UploadImageAsync(model.ImageProductFile, "Produtos");                
                }                               
                var produto = _converterHelper.ToProduct(model, path, true);

                //TODO: atribuir o usuário logado
                produto.Usuario = await _usuarioHelper.GetUserByEmailAsync(User.Identity.Name);
                await _produtoRepository.CreateAsync(produto); // ✅ agora usa método genérico
                return RedirectToAction(nameof(Index));
            }
            // ⚠️ repetir SelectList caso o ModelState seja inválido
            ViewData["CategoriaProdutoId"] = new SelectList(_categoriaProdutoRepository.GetAll(), "CategoriaProdutoId", "Nome", model.CategoriaProdutoId);
            ViewData["FornecedorId"] = new SelectList(_fornecedorRepository.GetAll(), "FornecedorId", "Nome", model.FornecedorId);
            return View(model);
        }

        

        // =======================
        // GET: Produtos/Edit/5
        // =======================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _produtoRepository.GetByIdAsync(id.Value);
            if (produto == null) return NotFound();


            var model = _converterHelper.ToProductViewModel(produto);

            ViewData["CategoriaProdutoId"] = new SelectList(_categoriaProdutoRepository.GetAll(), "CategoriaProdutoId", "Nome", model.CategoriaProdutoId);
            ViewData["FornecedorId"] = new SelectList(_fornecedorRepository.GetAll(), "FornecedorId", "Nome", model.FornecedorId);
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
                    var path = model.Foto; // manter a foto atual se não for alterada
                    if (model.ImageProductFile != null && model.ImageProductFile.Length > 0)
                    {

                        path = await _imageHelper.UploadImageAsync(model.ImageProductFile, "Produtos");

                    }
                    var produto = _converterHelper.ToProduct(model, path, false);

                    //TODO: atribuir o usuário logado
                    produto.Usuario = await _usuarioHelper.GetUserByEmailAsync(User.Identity.Name);
                    await _produtoRepository.UpdateAsync(produto); // ✅ genérico cobre Update
                }
                catch
                {
                    if (!await _produtoRepository.ExistsAsync(model.ProdutoId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaProdutoId"] = new SelectList(_categoriaProdutoRepository.GetAll(), "CategoriaProdutoId", "Nome", model.CategoriaProdutoId);
            ViewData["FornecedorId"] = new SelectList(_fornecedorRepository.GetAll(), "FornecedorId", "Nome", model.FornecedorId);
            return View(model);
        }

        // =======================
        // GET: Produtos/Delete/5
        // =======================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var produto = await _produtoRepository.GetByIdAsync(id.Value);
            if (produto == null) return NotFound();

            return View(produto);
        }

        // =======================
        // POST: Produtos/Delete/5
        // =======================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _produtoRepository.GetByIdAsync(id);
            await _produtoRepository.DeleteAsync(produto);
            return RedirectToAction(nameof(Index));
        }

    }
}
