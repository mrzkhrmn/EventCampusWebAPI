using EventCampusAPI.Repositories;

namespace EventCampusAPI.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IEventRepository Events { get; }
        IEventParticipantRepository EventParticipants { get; }
        ICategoryRepository Categories { get; }
        IUniversityRepository Universities { get; }
        IFacultyRepository Faculties { get; }
        IDepartmentRepository Departments { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        bool HasActiveTransaction { get; }
    }
} 