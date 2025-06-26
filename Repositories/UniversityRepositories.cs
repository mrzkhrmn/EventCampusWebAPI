using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventCampusAPI.Repositories
{
    public class UniversityRepository : GenericRepository<University>, IUniversityRepository
    {
        public UniversityRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<University?> GetUniversityWithFacultiesAsync(int universityId)
        {
            return await _dbSet
                .Include(u => u.Faculties)
                    .ThenInclude(f => f.Departments)
                .FirstOrDefaultAsync(u => u.Id == universityId);
        }

        public async Task<IEnumerable<University>> GetActiveUniversitiesAsync()
        {
            return await _dbSet
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
    }

    public class FacultyRepository : GenericRepository<Faculty>, IFacultyRepository
    {
        public FacultyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Faculty>> GetFacultiesByUniversityAsync(int universityId)
        {
            return await _dbSet
                .Where(f => f.UniversityId == universityId)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task<Faculty?> GetFacultyWithDepartmentsAsync(int facultyId)
        {
            return await _dbSet
                .Include(f => f.Departments)
                .Include(f => f.University)
                .FirstOrDefaultAsync(f => f.Id == facultyId);
        }
    }

    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByFacultyAsync(int facultyId)
        {
            return await _dbSet
                .Where(d => d.FacultyId == facultyId)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Department>> GetDepartmentsByUniversityAsync(int universityId)
        {
            return await _dbSet
                .Include(d => d.Faculty)
                .Where(d => d.Faculty.UniversityId == universityId)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }
    }
} 