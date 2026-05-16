using Etkincity.Data;
using Etkincity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Etkincity.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // Dashboard
    public async Task<IActionResult> Index()
    {
        var totalEvents = await _context.Events.CountAsync();
        var totalReservations = await _context.Reservations.Where(r => r.IsPaid).CountAsync();
        var totalIncome = await _context.Reservations.Where(r => r.IsPaid).SumAsync(r => r.TotalPrice);

        var events = await _context.Events.OrderByDescending(e => e.Date).ToListAsync();

        ViewBag.TotalEvents = totalEvents;
        ViewBag.TotalReservations = totalReservations;
        ViewBag.TotalIncome = totalIncome;

        return View(events);
    }

    // GET: List Reservations
    public async Task<IActionResult> Reservations()
    {
        var reservations = await _context.Reservations
            .Where(r => r.IsPaid)
            .Include(r => r.Event)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();
            
        return View(reservations);
    }

    // Temporary: Database Review
    public async Task<IActionResult> DatabaseReview()
    {
        var events = await _context.Events.OrderBy(e => e.Category).ToListAsync();
        return View(events);
    }

    // POST: Fix all database images based on category with unique professional images
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FixEventImages()
    {
        var events = await _context.Events.ToListAsync();
        
        foreach (var evt in events)
        {
            // Picsum veya boş olanları profesyonel Unsplash linkleri ile değiştir
            // Her resme &sig=@evt.Id ekleyerek hepsinin benzersiz olmasını sağlıyoruz.
            // Önemli: Kullanıcının kendi eklediği/düzelttiği özel Unsplash görsellerini veya yüklediği yerel resimleri EZMEMEK için unsplash kontrolünü kaldırdık.
            if (string.IsNullOrEmpty(evt.ImageUrl) || evt.ImageUrl.Contains("picsum"))
            {
                string baseUrl = evt.Category switch
                {
                    EventCategory.Konser => "https://images.unsplash.com/photo-1501281668745-f7f57925c3b4",
                    EventCategory.Spor => "https://images.unsplash.com/photo-1504450758481-7338eba7524a",
                    EventCategory.Tiyatro => "https://images.unsplash.com/photo-1507676184212-d03ab07a01bf",
                    EventCategory.Festival => "https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3",
                    EventCategory.Sergi => "https://images.unsplash.com/photo-1460661419201-fd4cecdf8a8b",
                    EventCategory.Soylesi => "https://images.unsplash.com/photo-1475721027187-402ad2989a3b",
                    EventCategory.StandUp => "https://images.unsplash.com/photo-1527224857853-e3748b6f4813",
                    _ => "https://images.unsplash.com/photo-1492684223066-81342ee5ff30"
                };

                // ID bazlı benzersiz imza ekle
                evt.ImageUrl = $"{baseUrl}?auto=format&fit=crop&w=800&q=80&sig={evt.Id}";
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Tüm veritabanı görselleri her etkinlik için tamamen benzersiz olacak şekilde güncellendi!";
        return RedirectToAction(nameof(DatabaseReview));
    }

    // GET: Create Event
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create Event
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event evt)
    {
        if (ModelState.IsValid)
        {
            if (evt.ImageUpload != null)
            {
                Console.WriteLine("DEBUG: Dosya Geldi -> " + evt.ImageUpload.FileName);
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + evt.ImageUpload.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await evt.ImageUpload.CopyToAsync(fileStream);
                }
                
                evt.ImageUrl = "/images/" + uniqueFileName;
                Console.WriteLine("DEBUG: Yeni URL Atandı -> " + evt.ImageUrl);
            }
            else
            {
                Console.WriteLine("DEBUG: ImageUpload NULL!");
            }

            _context.Events.Add(evt);
            await _context.SaveChangesAsync();
            Console.WriteLine("DEBUG: Veritabanına Kaydedildi. ID: " + evt.Id);
            
            TempData["SuccessMessage"] = "Yeni etkinlik başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            Console.WriteLine("DEBUG: ModelState Geçersiz!");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("DEBUG: Hata -> " + error.ErrorMessage);
            }
        }
        return View(evt);
    }
    // GET: Edit Event
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var evt = await _context.Events.FindAsync(id);
        if (evt == null) return NotFound();

        return View(evt);
    }

    // POST: Edit Event
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event evt)
    {
        if (id != evt.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
                if (existingEvent == null) return NotFound();

                if (evt.ImageUpload != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + evt.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await evt.ImageUpload.CopyToAsync(fileStream);
                    }
                    evt.ImageUrl = "/images/" + uniqueFileName;
                }
                else
                {
                    // Yeni fotoğraf yüklenmediyse, mevcut fotoğrafı koru
                    evt.ImageUrl = existingEvent.ImageUrl;
                }

                _context.Update(evt);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(evt.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(evt);
    }

    private bool EventExists(int id)
    {
        return _context.Events.Any(e => e.Id == id);
    }

    // POST: Delete Event
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var evt = await _context.Events.FindAsync(id);
        if (evt != null)
        {
            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Etkinlik sistemden silindi.";
        }
        return RedirectToAction(nameof(Index));
    }
}
