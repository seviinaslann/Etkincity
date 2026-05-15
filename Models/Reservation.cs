using System.ComponentModel.DataAnnotations;

namespace Etkincity.Models;

public class Reservation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }
    
    public Event? Event { get; set; }

    [Required(ErrorMessage = "Lütfen adınızı giriniz.")]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen e-posta adresinizi giriniz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    [Range(1, 1, ErrorMessage = "Sadece 1 bilet rezerve edebilirsiniz.")]
    public int TicketCount { get; set; }

    public string? UserId { get; set; }

    [Required(ErrorMessage = "Lütfen bir koltuk seçiniz.")]
    [StringLength(10)]
    public string SelectedSeat { get; set; } = string.Empty;

    public DateTime ReservationDate { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string ReservationCode { get; set; } = string.Empty;

    public bool IsPaid { get; set; } = false;

    public decimal TotalPrice { get; set; }
}
