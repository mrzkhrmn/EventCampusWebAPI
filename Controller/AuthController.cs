using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using EventCampusAPI.Models;
using EventCampusAPI.Entities;
using EventCampusAPI.Models.Exceptions;
using EventCampusAPI.UnitOfWork;
using EventCampusAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController>? _logger;
    private readonly TokenService _tokenService;

    public AuthController(
        IUnitOfWork unitOfWork, 
        IConfiguration configuration, 
        ILogger<AuthController>? logger,
        TokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        _tokenService = tokenService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestModel request)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new EmailAlreadyExistsException();
            }

            University university = null;
            if (request.UniversityId.HasValue)
            {
                university = await _unitOfWork.Universities.GetByIdAsync(request.UniversityId.Value);
                if (university == null)
                    throw new InvalidOperationException("Geçersiz üniversite seçimi.");
            }

            Faculty faculty = null;
            if (request.FacultyId.HasValue)
            {
                faculty = await _unitOfWork.Faculties.GetByIdAsync(request.FacultyId.Value);
                if (faculty == null)
                    throw new InvalidOperationException("Geçersiz fakülte seçimi.");
                
                if (request.UniversityId.HasValue && faculty.UniversityId != request.UniversityId.Value)
                    throw new InvalidOperationException("Seçilen fakülte, seçilen üniversiteye ait değil.");
            }

            Department department = null;
            if (request.DepartmentId.HasValue)
            {
                department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentId.Value);
                if (department == null)
                    throw new InvalidOperationException("Geçersiz bölüm seçimi.");
                
                if (request.FacultyId.HasValue && department.FacultyId != request.FacultyId.Value)
                    throw new InvalidOperationException("Seçilen bölüm, seçilen fakülteye ait değil.");
            }

            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                Password = request.Password, 
                UniversityId = request.UniversityId,
                FacultyId = request.FacultyId,
                DepartmentId = request.DepartmentId,
                ProfileImageUrl = null 
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            var token = _tokenService.GenerateToken(user);

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
        catch (EmailAlreadyExistsException)
        {
            if (_unitOfWork.HasActiveTransaction)
                await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        catch (InvalidOperationException)
        {
            if (_unitOfWork.HasActiveTransaction)
                await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
        catch (Exception ex)
        {
            if (_unitOfWork.HasActiveTransaction)
                await _unitOfWork.RollbackTransactionAsync();
            
            _logger?.LogError(ex, "Register işleminde beklenmeyen hata: {Message}", ex.Message);
            throw new InvalidOperationException("Kayıt işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.");
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(LoginRequestModel request)
    {
        var user = await _unitOfWork.Users.FindFirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);
        
        if (user == null)
            throw new UnauthorizedAccessException("Geçersiz email veya parola.");

        var userWithDetails = await _unitOfWork.Users.GetUserWithFullDetailsAsync(user.Id);
        
        var token = _tokenService.GenerateToken(user);

        var response = new LoginResponseModel
        {
            Token = token,
            UserInfo = new UserInfoModel
            {
                Id = userWithDetails.Id,
                Name = userWithDetails.Name,
                Surname = userWithDetails.Surname,
                Email = userWithDetails.Email,
                UniversityId = userWithDetails.UniversityId ?? 0,
                UniversityName = userWithDetails.University?.Name,
                FacultyId = userWithDetails.FacultyId,
                FacultyName = userWithDetails.Faculty?.Name,
                DepartmentId = userWithDetails.DepartmentId,
                DepartmentName = userWithDetails.Department?.Name,
                ProfileImageUrl = userWithDetails.ProfileImageUrl
            }
        };

        return Ok(response);
    }

    [HttpGet("GetToken")]
    public IActionResult GetToken()
    {
        var user = new User { Id = 1, Name = "testuser", Email = "test@test.com", Surname = "testsurname" };
        var token = _tokenService.GenerateToken(user);
        return Ok(token);
    }
}
