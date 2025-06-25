using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace EventCampusAPI.Entities
{
    public class Event
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        [Required]
        public DateTime EndDate { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [MaxLength(2000)]
        public string? Description { get; set; }
        
        // Resim bilgileri
        public string EventImagesJson { get; set; } = JsonSerializer.Serialize(new List<string> { "https://picsum.photos/200/300" });
        
        [NotMapped]
        public List<string> EventImages
        {
            get
            {
                try
                {
                    return string.IsNullOrEmpty(EventImagesJson) 
                        ? new List<string> { "https://picsum.photos/200/300" }
                        : JsonSerializer.Deserialize<List<string>>(EventImagesJson) ?? new List<string> { "https://picsum.photos/200/300" };
                }
                catch
                {
                    return new List<string> { "https://picsum.photos/200/300" };
                }
            }
            set
            {
                EventImagesJson = JsonSerializer.Serialize(value ?? new List<string> { "https://picsum.photos/200/300" });
            }
        }
        
        // Konum bilgileri
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        [MaxLength(500)]
        public string Address { get; set; }
        
        // Ücret bilgileri
        public bool IsFree { get; set; } = true;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Price { get; set; }
        
        // Kontenjan
        public int? MaxParticipants { get; set; }
        
        // Durumu
        public bool IsActive { get; set; } = true;
        public bool IsPublic { get; set; } = true;
        
        // Oluşturulma bilgileri
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Foreign Keys
        [Required]
        public int CategoryId { get; set; }
        
        [Required]
        public int CreatedByUserId { get; set; }
        
        [Required]
        public int UniversityId { get; set; }
        
        // Navigation Properties
        public Category Category { get; set; }
        public User CreatedByUser { get; set; }
        public University University { get; set; }
        
        // Katılımcılar
        public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
        
        // Computed properties
        [NotMapped]
        public int CurrentParticipantCount => Participants?.Count(p => p.IsConfirmed) ?? 0;
        
        [NotMapped]
        public bool IsRegistrationOpen => IsActive && 
            (MaxParticipants == null || CurrentParticipantCount < MaxParticipants) &&
            DateTime.UtcNow < StartDate;
            
        [NotMapped]
        public bool IsEventStarted => DateTime.UtcNow >= StartDate;
        
        [NotMapped]
        public bool IsEventEnded => DateTime.UtcNow >= EndDate;
    }
} 