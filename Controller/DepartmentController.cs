using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventCampusAPI.Data;
using EventCampusAPI.Entities;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly AppDbContext _context;

    public DepartmentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDepartments()
    {
        var departments = await _context.Departments
            .Include(d => d.Faculty)
            .ThenInclude(f => f.University)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.FacultyId,
                FacultyName = d.Faculty.Name,
                UniversityId = d.Faculty.UniversityId,
                UniversityName = d.Faculty.University.Name
            })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("by-faculty/{facultyId}")]
    public async Task<IActionResult> GetDepartmentsByFaculty(int facultyId)
    {
        var departments = await _context.Departments
            .Where(d => d.FacultyId == facultyId)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.FacultyId
            })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var department = await _context.Departments
            .Include(d => d.Faculty)
            .ThenInclude(f => f.University)
            .Where(d => d.Id == id)
            .Select(d => new
            {
                d.Id,
                d.Name,
                d.FacultyId,
                FacultyName = d.Faculty.Name,
                UniversityId = d.Faculty.UniversityId,
                UniversityName = d.Faculty.University.Name
            })
            .FirstOrDefaultAsync();

        if (department == null)
            return NotFound("Bölüm bulunamadı.");

        return Ok(department);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedDepartments()
    {
        try
        {
            var existingDepartments = await _context.Departments.CountAsync();
            if (existingDepartments > 0)
            {
                return BadRequest("Bölümler zaten mevcut. Önce mevcut verileri temizleyin.");
            }
        
            var faculties = await _context.Faculties.ToListAsync();
            if (!faculties.Any())
            {
                return BadRequest("Önce fakülte verilerini eklemeniz gerekiyor.");
            }

            var departments = new List<Department>();

            foreach (var faculty in faculties)
            {
                switch (faculty.Name)
                {
                    case "Mühendislik Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Bilgisayar Mühendisliği", FacultyId = faculty.Id },
                            new Department { Name = "Elektrik-Elektronik Mühendisliği", FacultyId = faculty.Id },
                            new Department { Name = "Makine Mühendisliği", FacultyId = faculty.Id },
                            new Department { Name = "İnşaat Mühendisliği", FacultyId = faculty.Id },
                            new Department { Name = "Endüstri Mühendisliği", FacultyId = faculty.Id },
                            new Department { Name = "Yazılım Mühendisliği", FacultyId = faculty.Id }
                        });
                        break;

                    case "Tıp Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Tıp", FacultyId = faculty.Id },
                            new Department { Name = "Hemşirelik", FacultyId = faculty.Id },
                            new Department { Name = "Fizyoterapi ve Rehabilitasyon", FacultyId = faculty.Id },
                            new Department { Name = "Beslenme ve Diyetetik", FacultyId = faculty.Id }
                        });
                        break;

                    case "Fen Bilimleri Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Matematik", FacultyId = faculty.Id },
                            new Department { Name = "Fizik", FacultyId = faculty.Id },
                            new Department { Name = "Kimya", FacultyId = faculty.Id },
                            new Department { Name = "Biyoloji", FacultyId = faculty.Id },
                            new Department { Name = "İstatistik", FacultyId = faculty.Id }
                        });
                        break;

                    case "Sosyal Bilimler Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Psikoloji", FacultyId = faculty.Id },
                            new Department { Name = "Sosyoloji", FacultyId = faculty.Id },
                            new Department { Name = "Tarih", FacultyId = faculty.Id },
                            new Department { Name = "Felsefe", FacultyId = faculty.Id },
                            new Department { Name = "Sanat Tarihi", FacultyId = faculty.Id }
                        });
                        break;

                    case "İktisadi ve İdari Bilimler Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "İşletme", FacultyId = faculty.Id },
                            new Department { Name = "Ekonomi", FacultyId = faculty.Id },
                            new Department { Name = "Uluslararası İlişkiler", FacultyId = faculty.Id },
                            new Department { Name = "Kamu Yönetimi", FacultyId = faculty.Id },
                            new Department { Name = "Maliye", FacultyId = faculty.Id }
                        });
                        break;

                    case "Eğitim Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Sınıf Öğretmenliği", FacultyId = faculty.Id },
                            new Department { Name = "Matematik Öğretmenliği", FacultyId = faculty.Id },
                            new Department { Name = "Fen Bilgisi Öğretmenliği", FacultyId = faculty.Id },
                            new Department { Name = "İngilizce Öğretmenliği", FacultyId = faculty.Id },
                            new Department { Name = "Okul Öncesi Öğretmenliği", FacultyId = faculty.Id }
                        });
                        break;

                    case "Hukuk Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Hukuk", FacultyId = faculty.Id }
                        });
                        break;

                    case "Mimarlık ve Tasarım Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Mimarlık", FacultyId = faculty.Id },
                            new Department { Name = "İç Mimarlık", FacultyId = faculty.Id },
                            new Department { Name = "Endüstriyel Tasarım", FacultyId = faculty.Id },
                            new Department { Name = "Peyzaj Mimarlığı", FacultyId = faculty.Id }
                        });
                        break;

                    case "İletişim Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Halkla İlişkiler ve Tanıtım", FacultyId = faculty.Id },
                            new Department { Name = "Gazetecilik", FacultyId = faculty.Id },
                            new Department { Name = "Radyo, Televizyon ve Sinema", FacultyId = faculty.Id },
                            new Department { Name = "Yeni Medya ve İletişim", FacultyId = faculty.Id }
                        });
                        break;

                    case "Güzel Sanatlar Fakültesi":
                        departments.AddRange(new List<Department>
                        {
                            new Department { Name = "Resim", FacultyId = faculty.Id },
                            new Department { Name = "Heykel", FacultyId = faculty.Id },
                            new Department { Name = "Müzik", FacultyId = faculty.Id },
                            new Department { Name = "Sahne Sanatları", FacultyId = faculty.Id }
                        });
                        break;
                }
            }

            await _context.Departments.AddRangeAsync(departments);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"{departments.Count} bölüm başarıyla eklendi.", Count = departments.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Seed işlemi sırasında hata oluştu.", Error = ex.Message });
        }
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearDepartments()
    {
        try
        {
            var departments = await _context.Departments.ToListAsync();
            _context.Departments.RemoveRange(departments);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Tüm bölümler silindi.", Count = departments.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Silme işlemi sırasında hata oluştu.", Error = ex.Message });
        }
    }
} 