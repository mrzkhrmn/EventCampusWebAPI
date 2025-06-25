namespace EventCampusAPI.Models
{
    public class EventResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Description { get; set; }
        
        public List<string> EventImages { get; set; } = new List<string>();
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Address { get; set; }
        public bool IsFree { get; set; }
        public decimal? Price { get; set; }
        public int? MaxParticipants { get; set; }
        public int CurrentParticipantCount { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public CategoryResponseModel Category { get; set; }
        public UserInfoModel CreatedByUser { get; set; }
        public UniversityResponseModel University { get; set; }
        
        public List<EventParticipantResponseModel> Participants { get; set; } = new List<EventParticipantResponseModel>();
        
        public bool IsRegistrationOpen { get; set; }
        public bool IsEventStarted { get; set; }
        public bool IsEventEnded { get; set; }
        public bool IsUserParticipant { get; set; } 
    }
    
    public class EventParticipantResponseModel
    {
        public int Id { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsConfirmed { get; set; }
        public UserInfoModel User { get; set; }
    }
    
    public class CategoryResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string? Description { get; set; }
    }
    
    public class UniversityResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
} 