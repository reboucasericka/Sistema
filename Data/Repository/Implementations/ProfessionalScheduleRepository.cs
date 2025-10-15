using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class ProfessionalScheduleRepository : GenericRepository<ProfessionalSchedule>, IProfessionalScheduleRepository
    {
        private readonly SistemaDbContext _context;

        public ProfessionalScheduleRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<ProfessionalSchedule> GetAllWithIncludes()
        {
            return _context.ProfessionalSchedules
                .Include(ps => ps.Professional)
                    .ThenInclude(p => p.User)
                .Include(ps => ps.User)
                .AsNoTracking();
        }

        public IQueryable<ProfessionalSchedule> GetByProfessionalId(int professionalId)
        {
            return _context.ProfessionalSchedules
                .Include(ps => ps.Professional)
                    .ThenInclude(p => p.User)
                .Include(ps => ps.User)
                .Where(ps => ps.ProfessionalId == professionalId)
                .AsNoTracking();
        }

        public IQueryable<ProfessionalSchedule> GetByDayOfWeek(DayOfWeek dayOfWeek)
        {
            return _context.ProfessionalSchedules
                .Include(ps => ps.Professional)
                    .ThenInclude(p => p.User)
                .Include(ps => ps.User)
                .Where(ps => ps.DayOfWeek == dayOfWeek)
                .AsNoTracking();
        }

        public async Task<ProfessionalSchedule?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.ProfessionalSchedules
                .Include(ps => ps.Professional)
                    .ThenInclude(p => p.User)
                .Include(ps => ps.User)
                .FirstOrDefaultAsync(ps => ps.ScheduleId == id);
        }
    }
}
