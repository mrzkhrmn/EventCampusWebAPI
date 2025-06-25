using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EventCampusAPI.Models;
using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using EventCampusAPI.Models.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController>? _logger;

    public AuthController(AppDbContext context, IConfiguration configuration, ILogger<AuthController>? logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel request)
    {
        try
        {
            // Email kontrolü
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException();
            }

            // Üniversite kontrolü
            University university = null;
            if (request.UniversityId.HasValue)
            {
                university = await _context.Universities.FindAsync(request.UniversityId.Value);
                if (university == null)
                    throw new InvalidOperationException("Geçersiz üniversite seçimi.");
            }

            // Fakulte kontrolü
            Faculty faculty = null;
            if (request.FacultyId.HasValue)
            {
                faculty = await _context.Faculties.FindAsync(request.FacultyId.Value);
                if (faculty == null)
                    throw new InvalidOperationException("Geçersiz fakülte seçimi.");
                
                // Fakultenin seçilen üniversiteye ait olup olmadığını kontrol et
                if (request.UniversityId.HasValue && faculty.UniversityId != request.UniversityId.Value)
                    throw new InvalidOperationException("Seçilen fakülte, seçilen üniversiteye ait değil.");
            }

            // Bölüm kontrolü
            Department department = null;
            if (request.DepartmentId.HasValue)
            {
                department = await _context.Departments.FindAsync(request.DepartmentId.Value);
                if (department == null)
                    throw new InvalidOperationException("Geçersiz bölüm seçimi.");
                
                // Bölümün seçilen fakülteye ait olup olmadığını kontrol et
                if (request.FacultyId.HasValue && department.FacultyId != request.FacultyId.Value)
                    throw new InvalidOperationException("Seçilen bölüm, seçilen fakülteye ait değil.");
            }

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                Password = request.Password, // Hashleyin!
                UniversityId = request.UniversityId,
                FacultyId = request.FacultyId,
                DepartmentId = request.DepartmentId,
                ProfileImageUrl = null // Default olarak boş
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            var response = new LoginResponseModel
            {
                Token = token,
                UserInfo = new UserInfoModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    UniversityId = user.UniversityId ?? 0,
                    UniversityName = university?.Name,
                    FacultyId = user.FacultyId,
                    FacultyName = faculty?.Name,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = department?.Name,
                    ProfileImageUrl = user.ProfileImageUrl
                }
            };

            return Ok(response);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) 
            when (ex.InnerException?.Message?.Contains("UNIQUE constraint failed") == true ||
                  ex.InnerException?.Message?.Contains("duplicate") == true)
        {
            // Veritabanı seviyesinde duplicate email durumu
            throw new EmailAlreadyExistsException();
        }
        catch (EmailAlreadyExistsException)
        {
            // EmailAlreadyExistsException'ları tekrar fırlat
            throw;
        }
        catch (InvalidOperationException)
        {
            // Zaten fırlatılmış InvalidOperationException'ları tekrar fırlat
            throw;
        }
        catch (Exception ex)
        {
            // Diğer beklenmeyen hatalar
            _logger?.LogError(ex, "Register işleminde beklenmeyen hata: {Message}", ex.Message);
            throw new InvalidOperationException("Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequestModel request)
    {
        var user = await _context.Users
            .Include(u => u.University)
            .Include(u => u.Faculty)
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

        if (user == null)
            throw new UnauthorizedAccessException("Geçersiz email veya parola.");

        var token = GenerateJwtToken(user);

        var response = new LoginResponseModel
        {
            Token = token,
            UserInfo = new UserInfoModel
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email,
                UniversityId = user.UniversityId ?? 0,
                UniversityName = user.University?.Name,
                FacultyId = user.FacultyId,
                FacultyName = user.Faculty?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                ProfileImageUrl = user.ProfileImageUrl
            }
        };

        return Ok(response);
    }

    [HttpGet("GetToken")]
    public IActionResult GetToken()
    {
        var user = new User { Id = 1, Name = "testuser" };
        var token = GenerateJwtToken(user);
        return Ok(token);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Surname, user.Surname)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _configuration["JWT:Issuer"],        
            Audience = _configuration["JWT:Audience"],    
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
