using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sistema.Models.Admin
{
    public class ServiceProfessionalViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;

        public List<int>? SelectedProfessionals { get; set; }
        public IEnumerable<SelectListItem>? AvailableProfessionals { get; set; }
    }
}
