using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Etkincity.Models;
using Etkincity.Data;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Security.Claims;


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
        if (User.Identity != null && User.Identity.IsAuthenticated)
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

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                var viewRecord = new UserEventView 
                { 
                    UserId = userId, 
                    EventId = id, 
                    ViewDate = DateTime.Now 
                };
                _context.UserEventViews.Add(viewRecord);
                await _context.SaveChangesAsync();
            }
        }

        ViewBag.Event = evt;
        return View(new Reservation { EventId = evt.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeReservation(Reservation reservation)
    {
        var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == reservation.EventId);
        if (evt == null) return NotFound();

        ModelState.Remove(nameof(reservation.ReservationCode));
        ModelState.Remove(nameof(reservation.Event));
        ModelState.Remove(nameof(reservation.ReservationDate));
        ModelState.Remove(nameof(reservation.IsPaid));
        ModelState.Remove(nameof(reservation.TotalPrice));

        if (ModelState.IsValid)
        {
            reservation.ReservationCode = "ETK-" + Guid.NewGuid().ToString("N").Substring(0, 7).ToUpper();
            reservation.TotalPrice = reservation.TicketCount * evt.Price;
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

        var model = new PaymentViewModel
        {
            ReservationId = reservation.Id,
            TotalAmount = reservation.TotalPrice
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
                // ETK-VIP ekstra bilet avantajı vb. olabilir, şimdilik fiyata etki etmesin.
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

        string qrText = $"Bilet Kodu: {reservation.ReservationCode}\nEtkinlik: {reservation.Event?.Title}\nİsim: {reservation.CustomerName}\nAdet: {reservation.TicketCount}";
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
        using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
        {
            byte[] qrCodeImage = qrCode.GetGraphic(20);
            ViewBag.QrCodeBase64 = Convert.ToBase64String(qrCodeImage);
        }

        return View(reservation);
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
