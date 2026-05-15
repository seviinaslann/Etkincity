using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Etkincity.Models;
using Etkincity.Data;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Security.Claims;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authorization;


namespace Etkincity.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string searchString)
    {
        var query = _context.Events.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            string searchLower = searchString.ToLower();
            query = query.Where(e => 
                e.Title.ToLower().Contains(searchLower) || 
                e.Location.ToLower().Contains(searchLower));
            
            ViewData["CurrentFilter"] = searchString;
        }

        var events = await query.OrderBy(e => e.Date).ToListAsync();

        // Recommendation Logic
        if (User.Identity != null && User.Identity.IsAuthenticated && !User.IsInRole("Admin"))
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var topCategories = await _context.UserEventViews
                    .Where(v => v.UserId == userId)
                    .Include(v => v.Event)
                    .Where(v => v.Event != null)
                    .GroupBy(v => v.Event!.Category)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(2)
                    .ToListAsync();

                if (topCategories.Any())
                {
                    EventCategory? cat1 = topCategories.Count > 0 ? topCategories[0] : null;
                    EventCategory? cat2 = topCategories.Count > 1 ? topCategories[1] : null;

                    var recommendedEvents = await _context.Events
                        .Where(e => (e.Category == cat1 || e.Category == cat2) && e.Date > DateTime.Now)
                        .OrderBy(e => e.Date)
                        .Take(3)
                        .ToListAsync();
                        
                    ViewBag.RecommendedEvents = recommendedEvents;
                }
            }
        }

        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (evt == null) return NotFound();

        var reservation = new Reservation { EventId = evt.Id, TicketCount = 1 };

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                // İzleme kaydı
                var viewRecord = new UserEventView 
                { 
                    UserId = userId, 
                    EventId = id, 
                    ViewDate = DateTime.Now 
                };
                _context.UserEventViews.Add(viewRecord);
                await _context.SaveChangesAsync();

                // Kullanıcı bilgilerini önceden doldur
                reservation.CustomerEmail = User.Identity.Name ?? "";
                reservation.CustomerName = User.FindFirstValue("FullName") ?? "";
                reservation.UserId = userId;

                // Zaten ödenmiş rezervasyonu var mı kontrol et
                var paidReservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId && r.IsPaid == true);
                
                if (paidReservation != null)
                {
                    ViewBag.AlreadyReserved = true;
                    ViewBag.ReservationId = paidReservation.Id;
                }
                else
                {
                    // Ödenmemiş rezervasyon var mı kontrol et
                    var unpaidReservation = await _context.Reservations
                        .FirstOrDefaultAsync(r => r.EventId == id && r.UserId == userId && r.IsPaid == false);
                    if (unpaidReservation != null)
                    {
                        ViewBag.PendingPayment = true;
                        ViewBag.ReservationId = unpaidReservation.Id;
                    }
                }
            }
        }

        // Alınmış koltukları getir
        var takenSeats = await _context.Reservations
            .Where(r => r.EventId == id)
            .Select(r => r.SelectedSeat)
            .ToListAsync();
        
        ViewBag.TakenSeats = takenSeats;
        
        // 60 kapasite dolu mu?
        if (takenSeats.Count >= 60)
        {
            ViewBag.IsSoldOut = true;
        }

        ViewBag.Event = evt;
        return View(reservation);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeReservation(Reservation reservation)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == reservation.EventId);
        if (evt == null) return NotFound();

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        string userEmail = User.Identity?.Name ?? "";
        string userFullName = User.FindFirstValue("FullName") ?? "";

        // Bilgileri doğrula
        if (reservation.CustomerEmail != userEmail || reservation.CustomerName != userFullName)
        {
            ModelState.AddModelError(string.Empty, "Bilet alırken sadece kendi hesap bilgilerinizi kullanabilirsiniz.");
        }

        // Zaten ödenmiş rezervasyonu var mı kontrol et
        var existingPaid = await _context.Reservations
            .AnyAsync(r => r.EventId == reservation.EventId && r.UserId == userId && r.IsPaid == true);
        
        if (existingPaid)
        {
            ModelState.AddModelError(string.Empty, "Bu etkinlik için zaten satın alınmış bir biletiniz bulunmaktadır.");
        }

        // Eğer ödenmemiş bir rezervasyonu varsa, yenisini oluşturmak yerine doğrudan ödeme sayfasına yönlendir
        var existingUnpaid = await _context.Reservations
            .FirstOrDefaultAsync(r => r.EventId == reservation.EventId && r.UserId == userId && r.IsPaid == false);

        if (existingUnpaid != null && !existingPaid)
        {
            // Update the existing unpaid reservation with the newly selected seat if it's different and available
            if (!string.IsNullOrEmpty(reservation.SelectedSeat) && existingUnpaid.SelectedSeat != reservation.SelectedSeat)
            {
                var seatTaken = await _context.Reservations.AnyAsync(r => r.EventId == reservation.EventId && r.SelectedSeat == reservation.SelectedSeat && r.Id != existingUnpaid.Id);
                if (!seatTaken)
                {
                    existingUnpaid.SelectedSeat = reservation.SelectedSeat;
                    
                    decimal vipSurcharge = 0;
                    if (reservation.SelectedSeat.StartsWith("A") || reservation.SelectedSeat.StartsWith("B") || reservation.SelectedSeat.StartsWith("C"))
                    {
                        vipSurcharge = evt.Price * 0.5m; // %50 VIP farkı
                    }
                    existingUnpaid.TotalPrice = evt.Price + vipSurcharge;
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Payment), new { id = existingUnpaid.Id });
        }

        // Koltuk doluluk kontrolü
        if (string.IsNullOrEmpty(reservation.SelectedSeat))
        {
            ModelState.AddModelError("SelectedSeat", "Lütfen bir koltuk seçiniz.");
        }
        else
        {
            var seatTaken = await _context.Reservations.AnyAsync(r => r.EventId == reservation.EventId && r.SelectedSeat == reservation.SelectedSeat);
            if (seatTaken)
            {
                ModelState.AddModelError("SelectedSeat", "Seçtiğiniz koltuk başka bir kullanıcı tarafından alınmıştır. Lütfen başka bir koltuk seçiniz.");
            }
        }

        // Bilet sayısını 1'e zorla
        reservation.TicketCount = 1;
        reservation.UserId = userId;

        ModelState.Remove(nameof(reservation.ReservationCode));
        ModelState.Remove(nameof(reservation.Event));
        ModelState.Remove(nameof(reservation.ReservationDate));
        ModelState.Remove(nameof(reservation.IsPaid));
        ModelState.Remove(nameof(reservation.TotalPrice));
        ModelState.Remove(nameof(reservation.UserId));
        ModelState.Remove(nameof(reservation.TicketCount));

        if (ModelState.IsValid)
        {
            reservation.ReservationCode = "ETK-" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
            
            // VIP Fiyat Hesaplama
            decimal vipSurcharge = 0;
            if (reservation.SelectedSeat.StartsWith("A") || reservation.SelectedSeat.StartsWith("B") || reservation.SelectedSeat.StartsWith("C"))
            {
                vipSurcharge = evt.Price * 0.5m; // %50 VIP farkı
            }
            reservation.TotalPrice = evt.Price + vipSurcharge;
            
            reservation.IsPaid = false;
            
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Payment), new { id = reservation.Id });
        }
        
        ViewBag.Event = evt;
        return View("Details", reservation);
    }

    [HttpGet]
    public async Task<IActionResult> Payment(int id)
    {
        var reservation = await _context.Reservations.Include(r => r.Event).FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null || reservation.IsPaid) return NotFound();

        bool isVip = !string.IsNullOrEmpty(reservation.SelectedSeat) && (reservation.SelectedSeat.StartsWith("A") || reservation.SelectedSeat.StartsWith("B") || reservation.SelectedSeat.StartsWith("C"));

        var model = new PaymentViewModel
        {
            ReservationId = reservation.Id,
            TotalAmount = reservation.TotalPrice,
            StandardPrice = reservation.Event?.Price ?? 0,
            IsVipSeat = isVip
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessPayment(PaymentViewModel model)
    {
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == model.ReservationId);
        if (reservation == null || reservation.IsPaid) return NotFound();

        if (ModelState.IsValid)
        {
            // İndirim Kodu Uygulama
            if (!string.IsNullOrEmpty(model.PromoCode))
            {
                string code = model.PromoCode.ToUpper();
                if (code == "ETK-10") reservation.TotalPrice *= 0.90m;
                else if (code == "ETK-20") reservation.TotalPrice *= 0.80m;
                else if (code == "ETK-50") reservation.TotalPrice *= 0.50m;
                else if (code == "ETK-FREE") reservation.TotalPrice = 0m;
                else if (code == "ETK-VIP")
                {
                    if (reservation.SelectedSeat != null && (reservation.SelectedSeat.StartsWith("A") || reservation.SelectedSeat.StartsWith("B") || reservation.SelectedSeat.StartsWith("C")))
                    {
                        // VIP koltuk ise, fiyati standart fiyata indir!
                        reservation.TotalPrice = reservation.Event?.Price ?? 0;
                    }
                }
                else
                {
                    ModelState.AddModelError("PromoCode", "Geçersiz veya süresi dolmuş indirim kodu.");
                    
                    bool isVip = !string.IsNullOrEmpty(reservation.SelectedSeat) && (reservation.SelectedSeat.StartsWith("A") || reservation.SelectedSeat.StartsWith("B") || reservation.SelectedSeat.StartsWith("C"));
                    model.StandardPrice = reservation.Event?.Price ?? 0;
                    model.IsVipSeat = isVip;
                    model.TotalAmount = reservation.TotalPrice;
                    
                    return View("Payment", model);
                }
            }

            // Mock ödeme doğrulama işlemi (Gerçekte burada banka API'si çağrılır)
            reservation.IsPaid = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ödemeniz başarıyla alındı. Biletiniz oluşturuldu.";
            return RedirectToAction(nameof(Ticket), new { id = reservation.Id });
        }

        return View("Payment", model);
    }

    public async Task<IActionResult> Ticket(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (reservation == null) return NotFound();

        // Ödeme yapılmamışsa bileti gösterme, ödeme sayfasına yönlendir
        if (!reservation.IsPaid) return RedirectToAction(nameof(Payment), new { id = reservation.Id });

        string qrText = $"Bilet Kodu: {reservation.ReservationCode}\nEtkinlik: {reservation.Event?.Title}\nİsim: {reservation.CustomerName}\nKoltuk: {reservation.SelectedSeat}";
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            ViewBag.QrCodeBase64 = Convert.ToBase64String(qrCodeImage);
        }

        return View(reservation);
    }

    public async Task<IActionResult> DownloadTicketPdf(int id)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var reservation = await _context.Reservations
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null) return NotFound();

        // QR Kod Oluşturma
        byte[] qrCodeImage;
        string qrText = $"Bilet Kodu: {reservation.ReservationCode}\nEtkinlik: {reservation.Event?.Title}\nİsim: {reservation.CustomerName}\nKoltuk: {reservation.SelectedSeat}";
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            qrCodeImage = qrCode.GetGraphic(20);
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Verdana));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("ETKİNCİTY").FontSize(24).SemiBold().FontColor("#2D5AF0");
                        col.Item().Text("Etkinlik Biletiniz").FontSize(12).FontColor(Colors.Grey.Medium);
                    });

                    row.ConstantItem(50).AlignRight().Text(reservation.Event?.Category.ToString()).FontSize(10).Italic();
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    // Etkinlik Adı
                    col.Item().PaddingBottom(10).Text(reservation.Event?.Title).FontSize(20).SemiBold();

                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Bilgi Tablosu
                    col.Item().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100);
                            columns.RelativeColumn();
                        });

                        table.Cell().Text("Tarih:").SemiBold();
                        table.Cell().Text(reservation.Event?.Date.ToString("dd MMMM yyyy HH:mm"));

                        table.Cell().Text("Mekan:").SemiBold();
                        table.Cell().Text(reservation.Event?.Location);

                        table.Cell().Text("Bilet Sahibi:").SemiBold();
                        table.Cell().Text(reservation.CustomerName);

                        table.Cell().Text("Koltuk:").SemiBold();
                        table.Cell().Text(reservation.SelectedSeat).FontColor("#2D5AF0").Bold();

                        table.Cell().Text("Bilet Kodu:").SemiBold();
                        table.Cell().Text(reservation.ReservationCode).FontColor(Colors.Red.Medium).Bold();
                    });

                    // QR Kod
                    col.Item().PaddingTop(30).AlignCenter().Width(150).Image(qrCodeImage);
                    
                    col.Item().AlignCenter().PaddingTop(10).Text("Giriş için QR kodu okutun").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Footer().AlignCenter().Column(col => 
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5).Text("Bu bilet dijital olarak oluşturulmuştur. Keyifli etkinlikler dileriz.").FontSize(8).Italic().FontColor(Colors.Grey.Medium);
                    col.Item().Text("www.etkincity.com").FontSize(8).FontColor("#2D5AF0");
                });
            });
        });

        using (var stream = new MemoryStream())
        {
            document.GeneratePdf(stream);
            return File(stream.ToArray(), "application/pdf", $"Bilet_{reservation.ReservationCode}.pdf");
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
