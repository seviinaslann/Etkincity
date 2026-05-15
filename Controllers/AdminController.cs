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
            }

            _context.Events.Add(evt);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Yeni etkinlik başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
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
