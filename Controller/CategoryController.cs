using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventCampusAPI.Data;
using EventCampusAPI.Entities;
using EventCampusAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventCampusAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();
                var categoryResponse = categories.Select(c => new CategoryResponseModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Description = c.Description
                }).ToList();

                return Ok(new { 
                    success = true, 
                    message = "Kategoriler başarıyla getirildi", 
                    data = categoryResponse 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Kategori bulunamadı" });
                }

                var categoryResponse = new CategoryResponseModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = category.Icon,
                    Description = category.Description
                };

                return Ok(new { 
                    success = true, 
                    message = "Kategori başarıyla getirildi", 
                    data = categoryResponse 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        [Authorize] // Sadece giriş yapan kullanıcılar kategori ekleyebilir
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestModel model)
        {
            try
            {
                var category = new Category
                {
                    Name = model.Name,
                    Icon = model.Icon,
                    Description = model.Description
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var categoryResponse = new CategoryResponseModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = category.Icon,
                    Description = category.Description
                };

                return Ok(new { 
                    success = true, 
                    message = "Kategori başarıyla oluşturuldu", 
                    data = categoryResponse 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize] // Sadece giriş yapan kullanıcılar kategori düzenleyebilir
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequestModel model)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Kategori bulunamadı" });
                }

                category.Name = model.Name;
                category.Icon = model.Icon;
                category.Description = model.Description;

                await _context.SaveChangesAsync();

                var categoryResponse = new CategoryResponseModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Icon = category.Icon,
                    Description = category.Description
                };

                return Ok(new { 
                    success = true, 
                    message = "Kategori başarıyla güncellendi", 
                    data = categoryResponse 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize] // Sadece giriş yapan kullanıcılar kategori silebilir
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Events)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound(new { success = false, message = "Kategori bulunamadı" });
                }

                // Kategoriye ait etkinlik varsa silme
                if (category.Events.Any())
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Bu kategoriye ait etkinlikler bulunduğu için kategori silinemez" 
                    });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = "Kategori başarıyla silindi" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedCategories()
        {
            try
            {
                // Zaten kategori varsa seed etme
                if (await _context.Categories.AnyAsync())
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Kategoriler zaten mevcut" 
                    });
                }

                var categories = new List<Category>
                {
                    new Category { Name = "Akademik", Icon = "🎓", Description = "Akademik etkinlikler, seminerler, konferanslar" },
                    new Category { Name = "Sosyal", Icon = "🎉", Description = "Sosyal etkinlikler, partiler, buluşmalar" },
                    new Category { Name = "Spor", Icon = "⚽", Description = "Spor etkinlikleri, müsabakalar, turnuvalar" },
                    new Category { Name = "Kültür", Icon = "🎭", Description = "Kültürel etkinlikler, sanat, müzik" },
                    new Category { Name = "Teknoloji", Icon = "💻", Description = "Teknoloji etkinlikleri, workshop'lar, hackathon'lar" },
                    new Category { Name = "Kariyer", Icon = "💼", Description = "Kariyer etkinlikleri, iş fuarları, networking" },
                    new Category { Name = "Eğlence", Icon = "🎪", Description = "Eğlence etkinlikleri, oyunlar, aktiviteler" },
                    new Category { Name = "Sağlık", Icon = "🏥", Description = "Sağlık ve wellness etkinlikleri" },
                    new Category { Name = "Gönüllülük", Icon = "🤝", Description = "Gönüllülük projeleri ve sosyal sorumluluk" },
                    new Category { Name = "Diğer", Icon = "📝", Description = "Diğer etkinlikler" }
                };

                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    message = $"{categories.Count} kategori başarıyla eklendi" 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
} 