using EventCampusAPI.Data;
using EventCampusAPI.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace EventCampusAPI.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repository instances
        private IUserRepository? _users;
        private IEventRepository? _events;
        private IEventParticipantRepository? _eventParticipants;
        private ICategoryRepository? _categories;
        private IUniversityRepository? _universities;
        private IFacultyRepository? _faculties;
        private IDepartmentRepository? _departments;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IUserRepository Users => 
            _users ??= new UserRepository(_context);

        public IEventRepository Events => 
            _events ??= new EventRepository(_context);

        public IEventParticipantRepository EventParticipants => 
            _eventParticipants ??= new EventParticipantRepository(_context);

        public ICategoryRepository Categories => 
            _categories ??= new CategoryRepository(_context);

        public IUniversityRepository Universities => 
            _universities ??= new UniversityRepository(_context);

        public IFacultyRepository Faculties => 
            _faculties ??= new FacultyRepository(_context);

        public IDepartmentRepository Departments => 
            _departments ??= new DepartmentRepository(_context);

        public bool HasActiveTransaction => _transaction != null;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Zaten aktif bir transaction var");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Commit edilecek aktif transaction yok");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("Rollback edilecek aktif transaction yok");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
} 