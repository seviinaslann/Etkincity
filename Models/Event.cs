using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Etkincity.Models;

public class Event
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Etkinlik adı zorunludur.")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih zorunludur.")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Konum/Mekan zorunludur.")]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Range(0, 100000, ErrorMessage = "Geçerli bir fiyat giriniz.")]
    public decimal Price { get; set; }

    [Required]
    public EventCategory Category { get; set; }

    public string? ImageUrl { get; set; }

    [NotMapped]
    public IFormFile? ImageUpload { get; set; }
}
