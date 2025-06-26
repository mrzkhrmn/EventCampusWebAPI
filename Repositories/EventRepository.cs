using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventCampusAPI.Repositories
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Event?> GetEventWithDetailsAsync(int eventId)
        {
            return await _dbSet
                .Include(e => e.Category)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.University)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Faculty)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Department)
                .Include(e => e.University)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u.University)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u.Faculty)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u.Department)
                .FirstOrDefaultAsync(e => e.Id == eventId);
        }

        public async Task<IEnumerable<Event>> GetEventsForUniversityAsync(int universityId, int? categoryId = null, int page = 1, int pageSize = 10)
        {
            var query = _dbSet
                .Include(e => e.Category)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.University)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Faculty)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Department)
                .Include(e => e.University)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .Where(e => e.IsActive && e.UniversityId == universityId);

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            return await query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            return await _dbSet
                .Include(e => e.Category)
                .Include(e => e.CreatedByUser)
                .Include(e => e.University)
                .Where(e => e.CategoryId == categoryId && e.IsActive)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByCreatorAsync(int creatorId, int page = 1, int pageSize = 10)
        {
            return await _dbSet
                .Include(e => e.Category)
                .Include(e => e.University)
                .Include(e => e.Participants)
                .Where(e => e.CreatedByUserId == creatorId)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUpcomingEventsAsync(int universityId, int page = 1, int pageSize = 10)
        {
            var today = DateTime.Today;
            return await _dbSet
                .Include(e => e.Category)
                .Include(e => e.CreatedByUser)
                .Include(e => e.University)
                .Where(e => e.UniversityId == universityId && e.IsActive && e.StartDate >= today)
                .OrderBy(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetUserParticipatedEventsAsync(int userId, int page = 1, int pageSize = 10)
        {
            var query = _dbSet
                .Include(e => e.Category)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.University)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Faculty)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Department)
                .Include(e => e.University)
                .Include(e => e.Participants)
                    .ThenInclude(p => p.User)
                .Where(e => e.Participants.Any(p => p.UserId == userId && p.IsConfirmed));

            return await query
                .OrderByDescending(e => e.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsUserParticipantAsync(int eventId, int userId)
        {
            return await _context.EventParticipants
                .AnyAsync(ep => ep.EventId == eventId && ep.UserId == userId && ep.IsConfirmed);
        }

        public async Task<int> GetParticipantCountAsync(int eventId)
        {
            return await _context.EventParticipants
                .CountAsync(ep => ep.EventId == eventId && ep.IsConfirmed);
        }
    }
} 