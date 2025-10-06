using System.ComponentModel.DataAnnotations;
using Sistema.Data.Entities;

namespace Sistema.Models.Admin
{
    public class ClientProfileViewModel
    {
        public User User { get; set; }
        public List<Appointment> Appointments { get; set; }
        public List<Appointment> PendingAppointments { get; set; }
        public List<Appointment> ConfirmedAppointments { get; set; }
        public List<Appointment> PastAppointments { get; set; }
    }

}
