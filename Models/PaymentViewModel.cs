using System.ComponentModel.DataAnnotations;

namespace Etkincity.Models;

public class PaymentViewModel
{
    public int ReservationId { get; set; }
    public decimal TotalAmount { get; set; }

    public string? PromoCode { get; set; }

    [Required(ErrorMessage = "Lütfen kart üzerindeki ismi giriniz.")]
    [StringLength(100)]
    public string CardName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen kart numarasını giriniz.")]
    [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Geçerli bir 16 haneli kart numarası giriniz.")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen son kullanma tarihini giriniz.")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Lütfen geçerli bir tarih giriniz (AA/YY)")]
    public string ExpirationDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lütfen güvenlik kodunu giriniz.")]
    [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Geçerli bir CVV giriniz.")]
    public string Cvv { get; set; } = string.Empty;
}
