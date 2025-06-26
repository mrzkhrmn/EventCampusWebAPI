using EventCampusAPI.Entities;

namespace EventCampusAPI.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<Category?> GetCategoryWithEventsAsync(int categoryId);
    }
} 