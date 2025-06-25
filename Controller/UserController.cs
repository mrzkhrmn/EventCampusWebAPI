using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using EventCampusAPI.Models;


namespace EventCampusAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Include(u => u.University)
                .Include(u => u.Faculty)
                .Include(u => u.Department)
                .Select(u => new { 
                    u.Id, 
                    u.Name, 
                    u.Surname, 
                    u.Email, 
                    UniversityId = u.UniversityId ?? 0,
                    UniversityName = u.University != null ? u.University.Name : null,
                    FacultyId = u.FacultyId,
                    FacultyName = u.Faculty != null ? u.Faculty.Name : null,
                    DepartmentId = u.DepartmentId,
                    DepartmentName = u.Department != null ? u.Department.Name : null,
                    u.ProfileImageUrl
                })
                .ToListAsync();

            return Ok(new { userData = users });
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequestModel request)
        {
            // Email kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { userData = "Bu email adresi zaten kullanılıyor." });

            // Üniversite kontrolü
            University? university = null;
            if (request.UniversityId.HasValue)
            {
                university = await _context.Universities.FindAsync(request.UniversityId.Value);
                if (university == null)
                    return BadRequest(new { userData = "Geçersiz üniversite seçimi." });
            }

            // Fakulte kontrolü
            Faculty? faculty = null;
            if (request.FacultyId.HasValue)
            {
                faculty = await _context.Faculties.FindAsync(request.FacultyId.Value);
                if (faculty == null)
                    return BadRequest(new { userData = "Geçersiz fakülte seçimi." });
                
                if (request.UniversityId.HasValue && faculty.UniversityId != request.UniversityId.Value)
                    return BadRequest(new { userData = "Seçilen fakülte, seçilen üniversiteye ait değil." });
            }

            // Bölüm kontrolü
            Department? department = null;
            if (request.DepartmentId.HasValue)
            {
                department = await _context.Departments.FindAsync(request.DepartmentId.Value);
                if (department == null)
                    return BadRequest(new { userData = "Geçersiz bölüm seçimi." });
                
                if (request.FacultyId.HasValue && department.FacultyId != request.FacultyId.Value)
                    return BadRequest(new { userData = "Seçilen bölüm, seçilen fakülteye ait değil." });
            }

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                Password = request.Password, // Gerçek uygulamada hash'leyin!
                UniversityId = request.UniversityId,
                FacultyId = request.FacultyId,
                DepartmentId = request.DepartmentId,
                ProfileImageUrl = null
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userInfo = new
            {
                user.Id,
                user.Name,
                user.Surname,
                user.Email,
                UniversityId = user.UniversityId ?? 0,
                UniversityName = university?.Name,
                FacultyId = user.FacultyId,
                FacultyName = faculty?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = department?.Name,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return Ok(new { userData = userInfo });
        }

        [HttpDelete("DeleteAll")]
        public async Task<IActionResult> DeleteAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                
                if (!users.Any())
                    return BadRequest(new { userData = "Silinecek kullanıcı bulunmuyor." });

                var userCount = users.Count;

                // Önce kullanıcıların katıldığı event kayıtlarını sil (EventParticipants)
                var participations = await _context.EventParticipants
                    .Where(ep => users.Select(u => u.Id).Contains(ep.UserId))
                    .ToListAsync();
                
                if (participations.Any())
                {
                    _context.EventParticipants.RemoveRange(participations);
                    await _context.SaveChangesAsync();
                }

                // Sonra kullanıcıların oluşturduğu eventleri sil
                var createdEvents = await _context.Events
                    .Where(e => users.Select(u => u.Id).Contains(e.CreatedByUserId))
                    .ToListAsync();
                
                if (createdEvents.Any())
                {
                    _context.Events.RemoveRange(createdEvents);
                    await _context.SaveChangesAsync();
                }

                // En son kullanıcıları sil
                _context.Users.RemoveRange(users);
                await _context.SaveChangesAsync();

                return Ok(new { userData = $"{userCount} kullanıcı ve bağlı tüm kayıtları başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { userData = $"Silme işlemi sırasında hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost("SeedUsers")]
        public async Task<IActionResult> SeedUsers()
        {
            if (await _context.Users.AnyAsync())
                return BadRequest(new { userData = "Kullanıcılar zaten mevcut." });

            // Önce üniversitelerin var olduğundan emin olalım
            var universities = await _context.Universities.ToListAsync();
            if (!universities.Any())
                return BadRequest(new { userData = "Önce üniversiteler oluşturulmalı." });

            // Fakülte verilerini de alalım
            var faculties = await _context.Faculties.Include(f => f.University).ToListAsync();
            if (!faculties.Any())
                return BadRequest(new { userData = "Önce fakülteler oluşturulmalı. /api/Faculty/seed endpoint'ini kullanın." });

            // Bölüm verilerini de alalım
            var departments = await _context.Departments.Include(d => d.Faculty).ToListAsync();
            if (!departments.Any())
                return BadRequest(new { userData = "Önce bölümler oluşturulmalı. /api/Department/seed endpoint'ini kullanın." });

            // İTÜ fakülte ve bölümlerini bulalım
            var ituUniversity = universities.FirstOrDefault(u => u.ShortName == "İTÜ");
            var ituEngFaculty = faculties.FirstOrDefault(f => f.UniversityId == ituUniversity?.Id && f.Name == "Mühendislik Fakültesi");
            var ituCompEngDept = departments.FirstOrDefault(d => d.FacultyId == ituEngFaculty?.Id && d.Name == "Bilgisayar Mühendisliği");
            var ituSoftEngDept = departments.FirstOrDefault(d => d.FacultyId == ituEngFaculty?.Id && d.Name == "Yazılım Mühendisliği");

            // BOĞAZI ÜNI fakülte ve bölümlerini bulalım
            var bounUniversity = universities.FirstOrDefault(u => u.ShortName == "BOÜN");
            var bounEngFaculty = faculties.FirstOrDefault(f => f.UniversityId == bounUniversity?.Id && f.Name == "Mühendislik Fakültesi");
            var bounCompEngDept = departments.FirstOrDefault(d => d.FacultyId == bounEngFaculty?.Id && d.Name == "Bilgisayar Mühendisliği");

            // ODTÜ fakülte ve bölümlerini bulalım
            var odtuUniversity = universities.FirstOrDefault(u => u.ShortName == "ODTÜ");
            var odtuEngFaculty = faculties.FirstOrDefault(f => f.UniversityId == odtuUniversity?.Id && f.Name == "Mühendislik Fakültesi");
            var odtuCompEngDept = departments.FirstOrDefault(d => d.FacultyId == odtuEngFaculty?.Id && d.Name == "Bilgisayar Mühendisliği");

            // BİLKENT fakülte ve bölümlerini bulalım
            var bilkentUniversity = universities.FirstOrDefault(u => u.ShortName == "BİLKENT");
            var bilkentEngFaculty = faculties.FirstOrDefault(f => f.UniversityId == bilkentUniversity?.Id && f.Name == "Mühendislik Fakültesi");
            var bilkentCompEngDept = departments.FirstOrDefault(d => d.FacultyId == bilkentEngFaculty?.Id && d.Name == "Bilgisayar Mühendisliği");

            // SABANCI ÜNI fakülte ve bölümlerini bulalım
            var sabancıUniversity = universities.FirstOrDefault(u => u.ShortName == "SU");
            var sabancıEngFaculty = faculties.FirstOrDefault(f => f.UniversityId == sabancıUniversity?.Id && f.Name == "Mühendislik Fakültesi");
            var sabancıCompEngDept = departments.FirstOrDefault(d => d.FacultyId == sabancıEngFaculty?.Id && d.Name == "Bilgisayar Mühendisliği");

            var sampleUsers = new List<User>
            {
                new User 
                { 
                    Name = "Nihle", 
                    Surname = "Kurnaz", 
                    Email = "nihle.kurnaz@itu.edu.tr", 
                    Password = "Test123!", 
                    UniversityId = ituUniversity?.Id,
                    FacultyId = ituEngFaculty?.Id,
                    DepartmentId = ituCompEngDept?.Id,
                    ProfileImageUrl = null
                },
                new User 
                { 
                    Name = "Mirza", 
                    Surname = "Kahraman", 
                    Email = "mirza.kahraman@itu.edu.tr", 
                    Password = "Test123!", 
                    UniversityId = ituUniversity?.Id,
                    FacultyId = ituEngFaculty?.Id,
                    DepartmentId = ituSoftEngDept?.Id,
                    ProfileImageUrl = null
                },
                new User 
                { 
                    Name = "Ahmet", 
                    Surname = "Yılmaz", 
                    Email = "ahmet.yilmaz@itu.edu.tr", 
                    Password = "Test123!", 
                    UniversityId = ituUniversity?.Id,
                    FacultyId = ituEngFaculty?.Id,
                    DepartmentId = ituCompEngDept?.Id,
                    ProfileImageUrl = null
                },
                new User 
                { 
                    Name = "Elif", 
                    Surname = "Kaya", 
                    Email = "elif.kaya@boun.edu.tr", 
                    Password = "Test123!", 
                    UniversityId = bounUniversity?.Id,
                    FacultyId = bounEngFaculty?.Id,
                    DepartmentId = bounCompEngDept?.Id,
                    ProfileImageUrl = null
                },
                new User 
                { 
                    Name = "Mehmet", 
                    Surname = "Öz", 
                    Email = "mehmet.oz@odtu.edu.tr", 
                    Password = "Test123!", 
                    UniversityId = odtuUniversity?.Id,
                    FacultyId = odtuEngFaculty?.Id,
                    DepartmentId = odtuCompEngDept?.Id,
                    ProfileImageUrl = null
                },
                new User 
                { 
                    Name = "Zeynep", 
                    Surname = "Demir", 
                    Email = "zeynep.demir@bilkent.edu.tr", 
                    Password = "Test123!", 
                    UniversityId = bilkentUniversity?.Id,
                    FacultyId = bilkentEngFaculty?.Id,
                    DepartmentId = bilkentCompEngDept?.Id,
                    ProfileImageUrl = null
                },
                new User 
                { 
                    Name = "Can", 
                    Surname = "Arslan", 
                    Email = "can.arslan@sabanciuniv.edu.tr", 
                    Password = "Test123!", 
                    UniversityId = sabancıUniversity?.Id,
                    FacultyId = sabancıEngFaculty?.Id,
                    DepartmentId = sabancıCompEngDept?.Id,
                    ProfileImageUrl = null
                }
            };

            _context.Users.AddRange(sampleUsers);
            await _context.SaveChangesAsync();

            return Ok(new { userData = $"{sampleUsers.Count} örnek kullanıcı başarıyla eklendi." });
        }
    }
}