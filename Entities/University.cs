using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Entities
{
    public class University
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        
        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
    }
}