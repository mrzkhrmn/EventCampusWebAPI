using EventCampusAPI.Entities;

namespace EventCampusAPI.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserWithUniversityAsync(int userId);
        Task<User?> GetUserWithFullDetailsAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task<IEnumerable<User>> GetUsersByUniversityAsync(int universityId);
        Task<IEnumerable<User>> GetUsersByFacultyAsync(int facultyId);
        Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId);
    }
} 