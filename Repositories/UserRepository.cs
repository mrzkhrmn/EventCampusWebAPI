using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventCampusAPI.Repositories
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetUserWithUniversityAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserWithFullDetailsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.University)
                .Include(u => u.Faculty)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersByUniversityAsync(int universityId)
        {
            return await _dbSet
                .Where(u => u.UniversityId == universityId)
                .Include(u => u.Faculty)
                .Include(u => u.Department)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByFacultyAsync(int facultyId)
        {
            return await _dbSet
                .Where(u => u.FacultyId == facultyId)
                .Include(u => u.University)
                .Include(u => u.Department)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(int departmentId)
        {
            return await _dbSet
                .Where(u => u.DepartmentId == departmentId)
                .Include(u => u.University)
                .Include(u => u.Faculty)
                .ToListAsync();
        }
    }
} 