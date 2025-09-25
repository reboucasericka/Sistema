using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace Sistema.Models
{
    public class ProdutoViewModel : Produto
    {
        [Display(Name = "Foto do Produto")]
        public IFormFile ImageProductFile { get; set; }
    }
}
