using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Admin
{
    public class ProductViewModel : Product
    {
        [Display(Name = "Foto do Produto")]
        public IFormFile? PhotoFile { get; set; }
    }
}
