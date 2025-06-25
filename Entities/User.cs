using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
        public int? UniversityId { get; set; }
        public University? University { get; set; }
        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public string? ProfileImageUrl { get; set; }
        
        // Navigation properties for Events
        public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
        public ICollection<EventParticipant> EventParticipations { get; set; } = new List<EventParticipant>();
    }
}