using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Sistema.Data.Entities;
using Sistema.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace Sistema.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public Produto ToProduct(ProductViewModel model, string path, bool isNew)
        {
            return new Produto
            {
                ProdutoId = isNew ? 0 : model.ProdutoId,
                Nome = model.Nome,
                Descricao = model.Descricao,
                CategoriaProdutoId = model.CategoriaProdutoId,
                ValorCompra = model.ValorCompra,
                ValorVenda = model.ValorVenda,
                Estoque = model.Estoque,
                Foto = path,
                NivelEstoqueMinimo = model.NivelEstoqueMinimo,
                FornecedorId = model.FornecedorId
            };
        }

        public ProductViewModel ToProductViewModel(Produto produto)
        {
            return new ProductViewModel
            {
                ProdutoId = produto.ProdutoId,
                Nome = produto.Nome,
                Descricao = produto.Descricao,
                CategoriaProdutoId = produto.CategoriaProdutoId,
                ValorCompra = produto.ValorCompra,
                ValorVenda = produto.ValorVenda,
                Estoque = produto.Estoque,
                NivelEstoqueMinimo = produto.NivelEstoqueMinimo,
                FornecedorId = produto.FornecedorId,
                Foto = produto.Foto

            };
        }
    }
}
