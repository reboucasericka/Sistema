using Sistema.Data.Entities;

namespace Sistema.Data.Repository.Interfaces
{
    public interface IProfessionalRepository : IGenericRepository<Professional>
    {
        IQueryable<Professional> GetAllWithUsers();
        IQueryable<Professional> GetAllWithIncludes();
        IQueryable<Professional> GetActiveProfessionals();
        Task<Professional?> GetByIdWithIncludesAsync(int id);
        IQueryable<Professional> GetBySpecialty(string specialty);
    }
}
