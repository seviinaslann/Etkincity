using Etkincity.Data;
using Etkincity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Etkincity.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Dashboard
    public async Task<IActionResult> Index()
    {
        var totalEvents = await _context.Events.CountAsync();
        var totalReservations = await _context.Reservations.CountAsync();
        var totalIncome = await _context.Reservations.Where(r => r.IsPaid).SumAsync(r => r.TotalPrice);

        var events = await _context.Events.OrderByDescending(e => e.Date).ToListAsync();

        ViewBag.TotalEvents = totalEvents;
        ViewBag.TotalReservations = totalReservations;
        ViewBag.TotalIncome = totalIncome;

        return View(events);
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
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Yeni etkinlik başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(evt);
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
