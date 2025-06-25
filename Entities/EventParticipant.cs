using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Entities
{
    public class EventParticipant
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsConfirmed { get; set; } = false;
        
        // Navigation properties
        public Event Event { get; set; }
        public User User { get; set; }
    }
} 