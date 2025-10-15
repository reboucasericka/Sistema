using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IProfessionalScheduleRepository : IGenericRepository<ProfessionalSchedule>
    {
        IQueryable<ProfessionalSchedule> GetAllWithIncludes();
        IQueryable<ProfessionalSchedule> GetByProfessionalId(int professionalId);
        IQueryable<ProfessionalSchedule> GetByDayOfWeek(DayOfWeek dayOfWeek);
        Task<ProfessionalSchedule?> GetByIdWithIncludesAsync(int id);
    }
}
