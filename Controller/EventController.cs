using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventCampusAPI.Services;
using EventCampusAPI.Models;
using System.Security.Claims;

namespace EventCampusAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;

        public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Geçersiz kullanıcı tokeni");
            }
            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var eventResponse = await _eventService.CreateEventAsync(model, userId);
                
                return Ok(new { 
                    success = true, 
                    message = "Etkinlik başarıyla oluşturuldu", 
                    data = eventResponse 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(
            [FromQuery] int? categoryId = null,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var events = await _eventService.GetEventsForUserAsync(userId, categoryId, page, pageSize);
                
                string message = categoryId.HasValue 
                    ? "Kategoriye ait etkinlikler başarıyla getirildi" 
                    : "Etkinlikler başarıyla getirildi";
                
                return Ok(new { 
                    success = true, 
                    message = message, 
                    data = events,
                    categoryId = categoryId,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GetEventById/{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var eventResponse = await _eventService.GetEventByIdAsync(id, userId);
                
                if (eventResponse == null)
                {
                    return NotFound(new { success = false, message = "Etkinlik bulunamadı" });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Etkinlik başarıyla getirildi", 
                    data = eventResponse 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("GetParticipatedEvents/{id}")]
        public async Task<IActionResult> GetParticipatedEvents([FromRoute]int id,[FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var events = await _eventService.GetUserParticipatedEventsAsync(id, page, pageSize);
                
                return Ok(new { 
                    success = true, 
                    message = "Katıldığınız etkinlikler başarıyla getirildi", 
                    data = events,
                    page = page,
                    pageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> JoinEvent(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.JoinEventAsync(id, userId);
                
                if (result.Success)
                {
                    return Ok(new { success = true, message = "Etkinliğe başarıyla katıldınız" });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("{id}/leave")]
        public async Task<IActionResult> LeaveEvent(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _eventService.LeaveEventAsync(id, userId);
                
                if (result)
                {
                    return Ok(new { success = true, messsage = "Etkinlikten başarıyla ayrıldınız" });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Etkinlikten ayrılma işlemi başarısız" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
} 