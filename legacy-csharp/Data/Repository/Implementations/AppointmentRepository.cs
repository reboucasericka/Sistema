using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        private readonly SistemaDbContext _context;

        public AppointmentRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Appointment> GetAllWithIncludes()
        {
            return _context.Appointments
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
                .Include(a => a.Professional)
                    .ThenInclude(p => p.User)
                .Include(a => a.Service)
                    .ThenInclude(s => s.Category)
                .AsNoTracking();
        }

        public IQueryable<Appointment> GetByProfessionalId(int professionalId)
        {
            return _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Service)
                .Where(a => a.ProfessionalId == professionalId)
                .AsNoTracking();
        }

        public IQueryable<Appointment> GetByCustomerId(int customerId)
        {
            return _context.Appointments
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .Where(a => a.CustomerId == customerId)
                .AsNoTracking();
        }

        public IQueryable<Appointment> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Professional)
                .Include(a => a.Service)
                .Where(a => a.StartTime >= startDate && a.StartTime <= endDate)
                .AsNoTracking();
        }

        public IQueryable<Appointment> GetAvailableAppointments(int professionalId, DateTime date)
        {
            return _context.Appointments
                .Include(a => a.Customer)
                .Include(a => a.Service)
                .Where(a => a.ProfessionalId == professionalId && 
                           a.StartTime.Date == date.Date && 
                           a.Status != "Canceled")
                .AsNoTracking();
        }

        public async Task<Appointment?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Customer)
                    .ThenInclude(c => c.User)
                .Include(a => a.Professional)
                    .ThenInclude(p => p.User)
                .Include(a => a.Service)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int professionalId, DateTime startTime, DateTime endTime, int? excludeAppointmentId = null)
        {
            var query = _context.Appointments
                .Where(a => a.ProfessionalId == professionalId &&
                           a.Status != "Canceled" &&
                           ((a.StartTime < endTime && a.EndTime > startTime)));

            if (excludeAppointmentId.HasValue)
            {
                query = query.Where(a => a.AppointmentId != excludeAppointmentId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
