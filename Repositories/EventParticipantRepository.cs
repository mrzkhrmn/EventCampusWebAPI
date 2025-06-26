using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventCampusAPI.Repositories
{
    public class EventParticipantRepository : GenericRepository<EventParticipant>, IEventParticipantRepository
    {
        public EventParticipantRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<EventParticipant?> GetParticipationAsync(int eventId, int userId)
        {
            return await _dbSet
                .Include(ep => ep.Event)
                .Include(ep => ep.User)
                .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);
        }

        public async Task<IEnumerable<EventParticipant>> GetEventParticipantsAsync(int eventId)
        {
            return await _dbSet
                .Include(ep => ep.User)
                    .ThenInclude(u => u.University)
                .Include(ep => ep.User)
                    .ThenInclude(u => u.Faculty)
                .Include(ep => ep.User)
                    .ThenInclude(u => u.Department)
                .Where(ep => ep.EventId == eventId && ep.IsConfirmed)
                .OrderBy(ep => ep.JoinedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<EventParticipant>> GetUserParticipationsAsync(int userId)
        {
            return await _dbSet
                .Include(ep => ep.Event)
                    .ThenInclude(e => e.Category)
                .Include(ep => ep.Event)
                    .ThenInclude(e => e.University)
                .Where(ep => ep.UserId == userId && ep.IsConfirmed)
                .OrderByDescending(ep => ep.JoinedAt)
                .ToListAsync();
        }

        public async Task<bool> IsUserParticipantAsync(int eventId, int userId)
        {
            return await _dbSet
                .AnyAsync(ep => ep.EventId == eventId && ep.UserId == userId && ep.IsConfirmed);
        }

        public async Task<int> GetEventParticipantCountAsync(int eventId)
        {
            return await _dbSet
                .CountAsync(ep => ep.EventId == eventId && ep.IsConfirmed);
        }
    }
} 