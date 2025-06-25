using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Entities
{
    public class Faculty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UniversityId { get; set; }
        public University University { get; set; }
        
        // Navigation properties
        public ICollection<Department> Departments { get; set; } = new List<Department>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
} 