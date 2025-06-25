using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string? Description { get; set; }
        
        // Navigation properties
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
} 