using System.ComponentModel.DataAnnotations;

namespace EventCampusAPI.Models
{
    public class CreateCategoryRequestModel
    {
        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Kategori ikonu zorunludur")]
        [MaxLength(10, ErrorMessage = "Kategori ikonu en fazla 10 karakter olabilir")]
        public string Icon { get; set; }
        
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }
    }
    
    public class UpdateCategoryRequestModel
    {
        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [MaxLength(100, ErrorMessage = "Kategori adı en fazla 100 karakter olabilir")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Kategori ikonu zorunludur")]
        [MaxLength(10, ErrorMessage = "Kategori ikonu en fazla 10 karakter olabilir")]
        public string Icon { get; set; }
        
        [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string? Description { get; set; }
    }
} 