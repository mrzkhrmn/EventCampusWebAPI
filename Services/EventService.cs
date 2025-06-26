using EventCampusAPI.Entities;
using EventCampusAPI.Models;
using EventCampusAPI.UnitOfWork;

namespace EventCampusAPI.Services
{
    public class EventService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EventResponseModel> CreateEventAsync(CreateEventRequestModel model, int userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Kullanıcının üniversite bilgisini al
                var user = await _unitOfWork.Users.GetUserWithFullDetailsAsync(userId);
                if (user?.UniversityId == null)
                    throw new InvalidOperationException("Etkinlik oluşturmak için üniversite bilginiz olmalı");

                // Kategori kontrolü
                var category = await _unitOfWork.Categories.GetByIdAsync(model.CategoryId);
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

                await _unitOfWork.Events.AddAsync(eventEntity);
                await _unitOfWork.SaveChangesAsync();

                var participation = new EventParticipant
                {
                    EventId = eventEntity.Id,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow,
                    IsConfirmed = true
                };

                await _unitOfWork.EventParticipants.AddAsync(participation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return await GetEventByIdAsync(eventEntity.Id, userId);
            }
            catch
            {
                if (_unitOfWork.HasActiveTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<EventResponseModel>> GetEventsForUserAsync(int userId, int? categoryId = null, int page = 1, int pageSize = 10)
        {
            var user = await _unitOfWork.Users.GetUserWithUniversityAsync(userId);
            if (user?.UniversityId == null)
                return new List<EventResponseModel>();

            var events = await _unitOfWork.Events.GetEventsForUniversityAsync(
                user.UniversityId.Value, categoryId, page, pageSize);

            var filteredEvents = new List<Event>();
            foreach (var eventItem in events)
            {
                var isParticipant = await _unitOfWork.EventParticipants.IsUserParticipantAsync(eventItem.Id, userId);
                if (!isParticipant)
                {
                    filteredEvents.Add(eventItem);
                }
            }

            return filteredEvents.Select(e => MapToResponseModel(e, userId)).ToList();
        }

        public async Task<EventResponseModel?> GetEventByIdAsync(int eventId, int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user?.UniversityId == null)
                return null;

            var eventEntity = await _unitOfWork.Events.GetEventWithDetailsAsync(eventId);
            if (eventEntity == null || eventEntity.UniversityId != user.UniversityId)
                return null;

            return MapToResponseModel(eventEntity, userId);
        }

        public async Task<(bool Success, string Message)> JoinEventAsync(int eventId, int userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user?.UniversityId == null)
                    return (false, "Etkinliğe katılmak için üniversite bilginiz olmalı");

                var eventEntity = await _unitOfWork.Events.GetEventWithDetailsAsync(eventId);
                if (eventEntity == null || eventEntity.UniversityId != user.UniversityId)
                    return (false, "Etkinlik bulunamadı veya farklı üniversiteye ait");

                if (!eventEntity.IsRegistrationOpen)
                {
                    if (eventEntity.IsEventStarted)
                        return (false, "Etkinlik başlamış, kayıt alımı kapanmış");
                    
                    var participantCount = await _unitOfWork.EventParticipants.GetEventParticipantCountAsync(eventId);
                    if (eventEntity.MaxParticipants != null && participantCount >= eventEntity.MaxParticipants)
                        return (false, "Etkinlik kontenjanı dolmuş");
                    
                    if (!eventEntity.IsActive)
                        return (false, "Etkinlik aktif değil");
                        
                    return (false, "Etkinlik kayıt alımına kapalı");
                }

                var existingParticipation = await _unitOfWork.EventParticipants.GetParticipationAsync(eventId, userId);
                if (existingParticipation != null)
                    return (false, "Bu etkinliğe zaten katılmışsınız");

                var participation = new EventParticipant
                {
                    EventId = eventId,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow,
                    IsConfirmed = true
                };

                await _unitOfWork.EventParticipants.AddAsync(participation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return (true, "Etkinliğe başarıyla katıldınız");
            }
            catch
            {
                if (_unitOfWork.HasActiveTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> LeaveEventAsync(int eventId, int userId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var participation = await _unitOfWork.EventParticipants.GetParticipationAsync(eventId, userId);
                if (participation == null)
                    return false;

                _unitOfWork.EventParticipants.Remove(participation);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                if (_unitOfWork.HasActiveTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<EventResponseModel>> GetUserParticipatedEventsAsync(int userId, int page = 1, int pageSize = 10)
        {
            var user = await _unitOfWork.Users.GetUserWithUniversityAsync(userId);
            if (user?.UniversityId == null)
                return new List<EventResponseModel>();

            var events = await _unitOfWork.Events.GetUserParticipatedEventsAsync(userId, page, pageSize);
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