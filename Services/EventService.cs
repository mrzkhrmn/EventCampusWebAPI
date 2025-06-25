using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using EventCampusAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCampusAPI.Services
{
    public class EventService
    {
        private readonly AppDbContext _context;

        public EventService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EventResponseModel> CreateEventAsync(CreateEventRequestModel model, int userId)
        {
            // Kullanıcının üniversite bilgisini al
            var user = await _context.Users
                .Include(u => u.University)
                .Include(u => u.Faculty)
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.UniversityId == null)
                throw new InvalidOperationException("Etkinlik oluşturmak için üniversite bilginiz olmalı");

            // Kategori kontrolü
            var category = await _context.Categories.FindAsync(model.CategoryId);
            if (category == null)
                throw new ArgumentException("Geçersiz kategori seçimi");

            var eventEntity = new Event
            {
                Name = model.Name,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                StartTime = model.StartTime.TimeOfDay,
                EndTime = model.EndTime.TimeOfDay,
                Description = model.Description,
                EventImages = model.EventImages ?? new List<string> { "https://picsum.photos/200/300" },
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Address = model.Address,
                IsFree = model.IsFree,
                Price = model.Price,
                MaxParticipants = model.MaxParticipants,
                CategoryId = model.CategoryId,
                CreatedByUserId = userId,
                UniversityId = user.UniversityId.Value,
                IsPublic = model.IsPublic,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(eventEntity);
            await _context.SaveChangesAsync();

            // Etkinlik oluşturan kişiyi otomatik olarak katılımcı olarak ekle
            var participation = new EventParticipant
            {
                EventId = eventEntity.Id,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                IsConfirmed = true
            };

            _context.EventParticipants.Add(participation);
            await _context.SaveChangesAsync();

            return await GetEventByIdAsync(eventEntity.Id, userId);
        }

        public async Task<List<EventResponseModel>> GetEventsForUserAsync(int userId, int? categoryId = null, int page = 1, int pageSize = 10)
        {
            // Kullanıcının üniversite bilgisini al
            var user = await _context.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.UniversityId == null)
                return new List<EventResponseModel>();

            var query = _context.Events
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
                .Where(e => e.IsActive && e.UniversityId == user.UniversityId)
                // Kullanıcının katılmadığı eventleri filtrele
                .Where(e => !e.Participants.Any(p => p.UserId == userId && p.IsConfirmed));

            // Kategori filtresi varsa uygula
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            query = query.OrderByDescending(e => e.CreatedAt)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .AsNoTracking(); // Tracking'i kapat, güncel veriyi çek

            var events = await query.ToListAsync();

            return events.Select(e => MapToResponseModel(e, userId)).ToList();
        }

        public async Task<EventResponseModel?> GetEventByIdAsync(int eventId, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.UniversityId == null)
                return null;

            var eventEntity = await _context.Events
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
                .Where(e => e.Id == eventId && e.UniversityId == user.UniversityId)
                .FirstOrDefaultAsync();

            return eventEntity != null ? MapToResponseModel(eventEntity, userId) : null;
        }

        public async Task<(bool Success, string Message)> JoinEventAsync(int eventId, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.UniversityId == null)
                return (false, "Etkinliğe katılmak için üniversite bilginiz olmalı");

            var eventEntity = await _context.Events
                .Include(e => e.Participants)
                .Where(e => e.Id == eventId && e.UniversityId == user.UniversityId)
                .FirstOrDefaultAsync();

            if (eventEntity == null)
                return (false, "Etkinlik bulunamadı veya farklı üniversiteye ait");

            if (!eventEntity.IsRegistrationOpen)
            {
                if (eventEntity.IsEventStarted)
                    return (false, "Etkinlik başlamış, kayıt alımı kapanmış");
                if (eventEntity.MaxParticipants != null && eventEntity.CurrentParticipantCount >= eventEntity.MaxParticipants)
                    return (false, "Etkinlik kontenjanı dolmuş");
                if (!eventEntity.IsActive)
                    return (false, "Etkinlik aktif değil");
                    
                return (false, "Etkinlik kayıt alımına kapalı");
            }

            // Kullanıcı zaten katılmış mı kontrol et
            var existingParticipation = await _context.EventParticipants
                .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);

            if (existingParticipation != null)
                return (false, "Bu etkinliğe zaten katılmışsınız");

            var participation = new EventParticipant
            {
                EventId = eventId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                IsConfirmed = true
            };

            _context.EventParticipants.Add(participation);
            await _context.SaveChangesAsync();

            return (true, "Etkinliğe başarıyla katıldınız");
        }

        public async Task<bool> LeaveEventAsync(int eventId, int userId)
        {
            var participation = await _context.EventParticipants
                .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.UserId == userId);

            if (participation == null)
                return false;

            _context.EventParticipants.Remove(participation);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<EventResponseModel>> GetUserParticipatedEventsAsync(int userId, int page = 1, int pageSize = 10)
        {
            // Kullanıcının üniversite bilgisini al
            var user = await _context.Users
                .Include(u => u.University)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.UniversityId == null)
                return new List<EventResponseModel>();

            var query = _context.Events
                .Include(e => e.Category)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.University)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Faculty)
                .Include(e => e.CreatedByUser)
                    .ThenInclude(u => u.Department)
                .Include(e => e.University)
                .Include(e => e.Participants)
                .Where(e => e.IsActive && e.UniversityId == user.UniversityId)
                // Sadece kullanıcının katıldığı eventleri getir
                .Where(e => e.Participants.Any(p => p.UserId == userId && p.IsConfirmed))
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var events = await query.ToListAsync();

            return events.Select(e => MapToResponseModel(e, userId)).ToList();
        }

        private EventResponseModel MapToResponseModel(Event eventEntity, int userId)
        {
            var isUserParticipant = eventEntity.Participants?.Any(p => p.UserId == userId && p.IsConfirmed) ?? false;

            return new EventResponseModel
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                StartTime = eventEntity.StartTime,
                EndTime = eventEntity.EndTime,
                Description = eventEntity.Description,
                EventImages = eventEntity.EventImages,
                Latitude = eventEntity.Latitude,
                Longitude = eventEntity.Longitude,
                Address = eventEntity.Address,
                IsFree = eventEntity.IsFree,
                Price = eventEntity.Price,
                MaxParticipants = eventEntity.MaxParticipants,
                CurrentParticipantCount = eventEntity.Participants?.Count(p => p.IsConfirmed) ?? 0,
                IsActive = eventEntity.IsActive,
                IsPublic = eventEntity.IsPublic,
                CreatedAt = eventEntity.CreatedAt,
                UpdatedAt = eventEntity.UpdatedAt,
                Category = new CategoryResponseModel
                {
                    Id = eventEntity.Category.Id,
                    Name = eventEntity.Category.Name,
                    Icon = eventEntity.Category.Icon,
                    Description = eventEntity.Category.Description
                },
                CreatedByUser = new UserInfoModel
                {
                    Id = eventEntity.CreatedByUser.Id,
                    Email = eventEntity.CreatedByUser.Email,
                    Name = eventEntity.CreatedByUser.Name,
                    Surname = eventEntity.CreatedByUser.Surname,
                    UniversityId = eventEntity.CreatedByUser.UniversityId ?? 0,
                    UniversityName = eventEntity.CreatedByUser.University?.Name,
                    FacultyId = eventEntity.CreatedByUser.FacultyId,
                    FacultyName = eventEntity.CreatedByUser.Faculty?.Name,
                    DepartmentId = eventEntity.CreatedByUser.DepartmentId,
                    DepartmentName = eventEntity.CreatedByUser.Department?.Name,
                    ProfileImageUrl = eventEntity.CreatedByUser.ProfileImageUrl
                },
                University = new UniversityResponseModel
                {
                    Id = eventEntity.University.Id,
                    Name = eventEntity.University.Name,
                    ShortName = eventEntity.University.ShortName
                },
                Participants = eventEntity.Participants?.Where(p => p.IsConfirmed).Select(p => new EventParticipantResponseModel
                {
                    Id = p.Id,
                    JoinedAt = p.JoinedAt,
                    IsConfirmed = p.IsConfirmed,
                    User = new UserInfoModel
                    {
                        Id = p.User.Id,
                        Email = p.User.Email,
                        Name = p.User.Name,
                        Surname = p.User.Surname,
                        UniversityId = p.User.UniversityId ?? 0,
                        UniversityName = p.User.University?.Name,
                        FacultyId = p.User.FacultyId,
                        FacultyName = p.User.Faculty?.Name,
                        DepartmentId = p.User.DepartmentId,
                        DepartmentName = p.User.Department?.Name,
                        ProfileImageUrl = p.User.ProfileImageUrl
                    }
                }).ToList() ?? new List<EventParticipantResponseModel>(),
                IsRegistrationOpen = eventEntity.IsRegistrationOpen,
                IsEventStarted = eventEntity.IsEventStarted,
                IsEventEnded = eventEntity.IsEventEnded,
                IsUserParticipant = isUserParticipant
            };
        }
    }
} 