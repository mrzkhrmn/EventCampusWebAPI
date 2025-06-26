using EventCampusAPI.Entities;

namespace EventCampusAPI.Repositories
{
    public interface IUniversityRepository : IGenericRepository<University>
    {
        Task<University?> GetUniversityWithFacultiesAsync(int universityId);
        Task<IEnumerable<University>> GetActiveUniversitiesAsync();
    }

    public interface IFacultyRepository : IGenericRepository<Faculty>
    {
        Task<IEnumerable<Faculty>> GetFacultiesByUniversityAsync(int universityId);
        Task<Faculty?> GetFacultyWithDepartmentsAsync(int facultyId);
    }

    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<IEnumerable<Department>> GetDepartmentsByFacultyAsync(int facultyId);
        Task<IEnumerable<Department>> GetDepartmentsByUniversityAsync(int universityId);
    }
} 