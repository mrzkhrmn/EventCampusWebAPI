using EventCampusAPI.Entities;

namespace EventCampusAPI.Repositories
{
    public interface IEventParticipantRepository : IGenericRepository<EventParticipant>
    {
        Task<EventParticipant?> GetParticipationAsync(int eventId, int userId);
        Task<IEnumerable<EventParticipant>> GetEventParticipantsAsync(int eventId);
        Task<IEnumerable<EventParticipant>> GetUserParticipationsAsync(int userId);
        Task<bool> IsUserParticipantAsync(int eventId, int userId);
        Task<int> GetEventParticipantCountAsync(int eventId);
    }
} 