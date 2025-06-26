using EventCampusAPI.Entities;

namespace EventCampusAPI.Repositories
{
    public interface IEventRepository : IGenericRepository<Event>
    {
        Task<Event?> GetEventWithDetailsAsync(int eventId);
        Task<IEnumerable<Event>> GetEventsForUniversityAsync(int universityId, int? categoryId = null, int page = 1, int pageSize = 10);
        Task<IEnumerable<Event>> GetEventsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Event>> GetEventsByCreatorAsync(int creatorId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Event>> GetUpcomingEventsAsync(int universityId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Event>> GetUserParticipatedEventsAsync(int userId, int page = 1, int pageSize = 10);
        Task<bool> IsUserParticipantAsync(int eventId, int userId);
        Task<int> GetParticipantCountAsync(int eventId);
    }
} 