using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;

namespace StudyMate.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    public ReviewController(ApplicationDbContext db, UserManager<ApplicationUser> users) { _db = db; _users = users; }

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Create(int bookingId)
    {
        var me = _users.GetUserId(User)!;
        var b = await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == bookingId && x.StudentId == me && x.Status == "Completed");
        if (b == null) return NotFound();
        if (await _db.Reviews.AnyAsync(r => r.BookingId == bookingId)) { TempData["Error"] = "Đã review."; return RedirectToAction("Index", "Booking"); }
        return View(new ReviewFormVm { BookingId = bookingId, Rating = 5 });
    }

    [Authorize(Roles = "Student")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewFormVm model)
    {
        var me = _users.GetUserId(User)!;
        var b = await _db.Bookings.FirstOrDefaultAsync(x => x.Id == model.BookingId && x.StudentId == me && x.Status == "Completed");
        if (b == null) return NotFound();
        if (model.Rating is < 1 or > 5) { ModelState.AddModelError(nameof(model.Rating), "1-5"); return View(model); }
        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == b.TutorId);
        if (profile == null) return BadRequest();

        _db.Reviews.Add(new Review {
            BookingId = b.Id, ReviewerId = me, TutorProfileId = profile.Id,
            Rating = model.Rating, Comment = model.Comment, CreatedAt = DateTime.UtcNow
        });
        var ratings = await _db.Reviews.Where(r => r.TutorProfileId == profile.Id).Select(r => (int)r.Rating).ToListAsync();
        ratings.Add(model.Rating);
        profile.TotalReviews = ratings.Count;
        profile.AverageRating = (decimal)ratings.Average();
        profile.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", "TutorProfile", new { id = profile.Id });
    }
}
public class ReviewFormVm {
    public int BookingId { get; set; }
    [Range(1,5)] public byte Rating { get; set; }
    public string? Comment { get; set; }
}
