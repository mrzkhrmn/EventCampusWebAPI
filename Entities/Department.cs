using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Entities
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }
        
        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
    }
} 