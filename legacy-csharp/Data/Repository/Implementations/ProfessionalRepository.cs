using Microsoft.EntityFrameworkCore;
using Sistema.Data.Entities;
using Sistema.Data.Repository.Interfaces;

namespace Sistema.Data.Repository.Implementations
{
    public class ProfessionalRepository : GenericRepository<Professional>, IProfessionalRepository
    {
        private readonly SistemaDbContext _context;

        public ProfessionalRepository(SistemaDbContext context) : base(context)
        {
            _context = context;
        }

        public IQueryable<Professional> GetAllWithUsers()
        {
            return _context.Professionals.Include(p => p.User);
        }

        public IQueryable<Professional> GetAllWithIncludes()
        {
            return _context.Professionals
                .Include(p => p.User)
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .Include(p => p.Schedules)
                .AsNoTracking();
        }

        public IQueryable<Professional> GetActiveProfessionals()
        {
            return _context.Professionals
                .Where(p => p.IsActive)
                .Include(p => p.User)
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .Include(p => p.Schedules)
                .AsNoTracking();
        }

        public async Task<Professional?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Professionals
                .Include(p => p.User)
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .Include(p => p.Schedules)
                .FirstOrDefaultAsync(p => p.ProfessionalId == id);
        }

        public IQueryable<Professional> GetBySpecialty(string specialty)
        {
            return _context.Professionals
                .Where(p => p.Specialty.Contains(specialty) && p.IsActive)
                .Include(p => p.User)
                .Include(p => p.ProfessionalServices)
                    .ThenInclude(ps => ps.Service)
                .AsNoTracking();
        }
    }
}
