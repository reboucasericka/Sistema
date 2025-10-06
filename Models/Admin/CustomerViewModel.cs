using Sistema.Data.Entities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sistema.Models.Admin
{
    public class CustomerViewModel : Customer
    {
        [Display(Name = "Foto do Cliente")]
        public IFormFile? PhotoFile { get; set; }

        // Computed properties for display
        public int TotalAppointments => Appointments?.Count ?? 0;
        public int ActiveAppointments => Appointments?.Count(a => a.Date >= DateTime.Today && a.Status != "Cancelado") ?? 0;
    }
}