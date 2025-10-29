using System.ComponentModel.DataAnnotations;
using Sistema.Data.Entities;
using SistemaAPI.DTOs;

namespace Sistema.Models.Admin
{
    public class AdminClientProfileViewModel
    {
        public User User { get; set; }
        public List<AppointmentDto> Appointments { get; set; } = new List<AppointmentDto>();
        public List<AppointmentDto> PendingAppointments { get; set; } = new List<AppointmentDto>();
        public List<AppointmentDto> ConfirmedAppointments { get; set; } = new List<AppointmentDto>();
        public List<AppointmentDto> PastAppointments { get; set; } = new List<AppointmentDto>();
    }

}
