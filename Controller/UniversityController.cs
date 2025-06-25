using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventCampusAPI.Data;
using EventCampusAPI.Entities;

namespace EventCampusAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UniversityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UniversityController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllUniversities()
        {
            var universities = await _context.Universities
                .Select(u => new { u.Id, u.Name, u.ShortName })
                .ToListAsync();

            return Ok(new { uniData = universities });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUniversity(int id)
        {
            var university = await _context.Universities.FindAsync(id);
            
            if (university == null)
                return NotFound("Üniversite bulunamadı.");

            return Ok(new { uniData = new { university.Id, university.Name, university.ShortName } });
        }

        [HttpPost("SeedData")]
        public async Task<IActionResult> SeedUniversities()
        {
            if (await _context.Universities.AnyAsync())
                return BadRequest("Üniversiteler zaten mevcut.");

            var universities = new List<University>
            {
                new University { Name = "İstanbul Teknik Üniversitesi", ShortName = "İTÜ" },
                new University { Name = "Boğaziçi Üniversitesi", ShortName = "BOÜN" },
                new University { Name = "Orta Doğu Teknik Üniversitesi", ShortName = "ODTÜ" },
                new University { Name = "Bilkent Üniversitesi", ShortName = "BİLKENT" },
                new University { Name = "Sabancı Üniversitesi", ShortName = "SU" },
                new University { Name = "Koç Üniversitesi", ShortName = "KU" },
                new University { Name = "Hacettepe Üniversitesi", ShortName = "HÜ" },
                new University { Name = "Ankara Üniversitesi", ShortName = "AÜ" },
                new University { Name = "İstanbul Üniversitesi", ShortName = "İÜ" },
                new University { Name = "Gazi Üniversitesi", ShortName = "GAÜN" }
            };

            _context.Universities.AddRange(universities);
            await _context.SaveChangesAsync();

            return Ok(new { uniData = "Üniversiteler başarıyla eklendi." });
        }
    }
} 