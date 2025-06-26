using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventCampusAPI.Entities;
using EventCampusAPI.Models;
using EventCampusAPI.UnitOfWork;

namespace EventCampusAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetActiveCategoriesAsync();
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
                var category = await _unitOfWork.Categories.GetByIdAsync(id);
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
        [Authorize] 
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequestModel model)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var category = new Category
                {
                    Name = model.Name,
                    Icon = model.Icon,
                    Description = model.Description
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

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
                if (_unitOfWork.HasActiveTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize] 
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequestModel model)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var category = await _unitOfWork.Categories.GetByIdAsync(id);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Kategori bulunamadı" });
                }

                category.Name = model.Name;
                category.Icon = model.Icon;
                category.Description = model.Description;

                _unitOfWork.Categories.Update(category);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

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
                if (_unitOfWork.HasActiveTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize] 
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var category = await _unitOfWork.Categories.GetCategoryWithEventsAsync(id);
                if (category == null)
                {
                    return NotFound(new { success = false, message = "Kategori bulunamadı" });
                }

                if (category.Events.Any())
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Bu kategoriye ait etkinlikler bulunduğu için kategori silinemez" 
                    });
                }

                _unitOfWork.Categories.Remove(category);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Ok(new { 
                    success = true, 
                    message = "Kategori başarıyla silindi" 
                });
            }
            catch (Exception ex)
            {
                if (_unitOfWork.HasActiveTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedCategories()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
        
                var existingCategories = await _unitOfWork.Categories.GetAllAsync();
                if (existingCategories.Any())
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

                await _unitOfWork.Categories.AddRangeAsync(categories);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return Ok(new { 
                    success = true, 
                    message = $"{categories.Count} kategori başarıyla eklendi" 
                });
            }
            catch (Exception ex)
            {
                if (_unitOfWork.HasActiveTransaction)
                    await _unitOfWork.RollbackTransactionAsync();
                return StatusCode(500, new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }
} 