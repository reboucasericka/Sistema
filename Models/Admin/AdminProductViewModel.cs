using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Admin
{
    public class AdminProductViewModel : Product
    {
        [Display(Name = "Foto do Produto")]
        public IFormFile? ImageFile { get; set; }
    }
}
