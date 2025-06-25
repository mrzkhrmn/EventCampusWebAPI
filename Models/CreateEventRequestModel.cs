using System.ComponentModel.DataAnnotations;

namespace EventCampusAPI.Models
{
    public class CreateEventRequestModel
    {
        [Required(ErrorMessage = "Etkinlik adı zorunludur")]
        [MaxLength(200, ErrorMessage = "Etkinlik adı en fazla 200 karakter olabilir")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
        public DateTime StartDate { get; set; }
        
        [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
        public DateTime EndDate { get; set; }
        
        [Required(ErrorMessage = "Başlangıç saati zorunludur")]
        public DateTime StartTime { get; set; }
        
        [Required(ErrorMessage = "Bitiş saati zorunludur")]
        public DateTime EndTime { get; set; }
        
        [MaxLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir")]
        public string? Description { get; set; }
        
        // Resim URL'leri
        public List<string>? EventImages { get; set; }
        
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        
        [Required(ErrorMessage = "Adres zorunludur")]
        [MaxLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string Address { get; set; }
        
        public bool IsFree { get; set; } = true;
        public decimal? Price { get; set; }
        public int? MaxParticipants { get; set; }
        
        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public int CategoryId { get; set; }
        
        public bool IsPublic { get; set; } = true;
    }
} 