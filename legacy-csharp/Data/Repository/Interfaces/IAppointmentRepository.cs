using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment>
    {
        IQueryable<Appointment> GetAllWithIncludes();
        IQueryable<Appointment> GetByProfessionalId(int professionalId);
        IQueryable<Appointment> GetByCustomerId(int customerId);
        IQueryable<Appointment> GetByDateRange(DateTime startDate, DateTime endDate);
        IQueryable<Appointment> GetAvailableAppointments(int professionalId, DateTime date);
        Task<Appointment?> GetByIdWithIncludesAsync(int id);
        Task<bool> IsTimeSlotAvailableAsync(int professionalId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null);
    }
}
