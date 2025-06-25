using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventCampusAPI.Data;
using EventCampusAPI.Entities;

[ApiController]
[Route("api/[controller]")]
public class FacultyController : ControllerBase
{
    private readonly AppDbContext _context;

    public FacultyController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllFaculties()
    {
        var faculties = await _context.Faculties
            .Include(f => f.University)
            .Select(f => new
            {
                f.Id,
                f.Name,
                f.UniversityId,
                UniversityName = f.University.Name
            })
            .ToListAsync();

        return Ok(faculties);
    }

    [HttpGet("by-university/{universityId}")]
    public async Task<IActionResult> GetFacultiesByUniversity(int universityId)
    {
        var faculties = await _context.Faculties
            .Where(f => f.UniversityId == universityId)
            .Select(f => new
            {
                f.Id,
                f.Name,
                f.UniversityId
            })
            .ToListAsync();

        return Ok(faculties);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFaculty(int id)
    {
        var faculty = await _context.Faculties
            .Include(f => f.University)
            .Where(f => f.Id == id)
            .Select(f => new
            {
                f.Id,
                f.Name,
                f.UniversityId,
                UniversityName = f.University.Name
            })
            .FirstOrDefaultAsync();

        if (faculty == null)
            return NotFound("Fakülte bulunamadı.");

        return Ok(faculty);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedFaculties()
    {
        try
        {
            // Önce mevcut fakülteleri kontrol et
            var existingFaculties = await _context.Faculties.CountAsync();
            if (existingFaculties > 0)
            {
                return BadRequest("Fakülteler zaten mevcut. Önce mevcut verileri temizleyin.");
            }

            // Üniversiteleri getir
            var universities = await _context.Universities.ToListAsync();
            if (!universities.Any())
            {
                return BadRequest("Önce üniversite verilerini eklemeniz gerekiyor.");
            }

            var faculties = new List<Faculty>();

            // Her üniversite için örnek fakülteler
            foreach (var university in universities)
            {
                faculties.AddRange(new List<Faculty>
                {
                    new Faculty { Name = "Mühendislik Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "Tıp Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "Fen Bilimleri Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "Sosyal Bilimler Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "İktisadi ve İdari Bilimler Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "Eğitim Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "Hukuk Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "Mimarlık ve Tasarım Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "İletişim Fakültesi", UniversityId = university.Id },
                    new Faculty { Name = "Güzel Sanatlar Fakültesi", UniversityId = university.Id }
                });
            }

            await _context.Faculties.AddRangeAsync(faculties);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{faculties.Count} fakülte başarıyla eklendi.", Count = faculties.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Seed işlemi sırasında hata oluştu.", Error = ex.Message });
        }
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearFaculties()
    {
        try
        {
            var faculties = await _context.Faculties.ToListAsync();
            _context.Faculties.RemoveRange(faculties);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tüm fakülteler silindi.", Count = faculties.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Silme işlemi sırasında hata oluştu.", Error = ex.Message });
        }
    }
} 