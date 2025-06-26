using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventCampusAPI.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryWithEventsAsync(int categoryId)
        {
            return await _dbSet
                .Include(c => c.Events)
                    .ThenInclude(e => e.University)
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }
    }
} 