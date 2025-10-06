using Sistema.Data.Entities;

namespace Sistema.Models.Admin
{

    /// <summary>
    /// ViewModel usado para transportar todos os dados do Dashboard
    /// de forma tipada, sem precisar usar ViewBag.
    /// </summary>
    public class DashboardViewModel
    {
        // Estatísticas gerais
        public int TotalAppointments { get; set; }
        public int TotalClients { get; set; }
        public int TotalProfessionals { get; set; }
        public int TotalServices { get; set; }
        public int TodayAppointments { get; set; }
        public int PendingAppointments { get; set; }

        // Listas de agendamentos
        public List<Appointment> TodayAppointmentsList { get; set; }
        public List<Appointment> UpcomingAppointmentsList { get; set; }
    }
}