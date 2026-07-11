using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.ViewModels.TutorProfile;

namespace StudyMate.Controllers;

/// <summary>
/// Quản lý hồ sơ gia sư (M2). Create/Edit/MyProfile — M2-T4.
/// </summary>
[Authorize(Roles = "Tutor")]
public class TutorProfileController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<TutorProfileController> _logger;

    public static readonly string[] EducationLevels =
        ["Cử nhân", "Thạc sĩ", "Tiến sĩ", "Khác"];

    public static readonly string[] TeachingModes =
        ["Online", "Offline", "Both"];

    public TutorProfileController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        ILogger<TutorProfileController> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>Dashboard hồ sơ — redirect Create nếu chưa có.</summary>
    [HttpGet]
    public async Task<IActionResult> MyProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var profile = await _db.TutorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (profile == null)
            return RedirectToAction(nameof(Create));

        var vm = MapToForm(profile, user);
        ViewData["UpdateSuccess"] = TempData["ProfileUpdateSuccess"];
        ViewData["CreateSuccess"] = TempData["ProfileCreateSuccess"];
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var exists = await _db.TutorProfiles.AnyAsync(p => p.UserId == user.Id);
        if (exists)
            return RedirectToAction(nameof(Edit));

        return View(new TutorProfileFormViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            IsAvailable = true,
            TeachingMode = "Online"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TutorProfileFormViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        if (await _db.TutorProfiles.AnyAsync(p => p.UserId == user.Id))
            return RedirectToAction(nameof(Edit));

        ValidateBusinessRules(model);

        if (!ModelState.IsValid)
        {
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;
            return View(model);
        }

        var entity = new TutorProfile
        {
            UserId = user.Id,
            Headline = model.Headline.Trim(),
            Bio = model.Bio.Trim(),
            YearsOfExperience = model.YearsOfExperience,
            EducationLevel = model.EducationLevel,
            HourlyRate = model.HourlyRate,
            TeachingMode = model.TeachingMode,
            Address = NormalizeAddress(model),
            IsAvailable = model.IsAvailable,
            IsVerified = false,
            AverageRating = 0,
            TotalReviews = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.TutorProfiles.Add(entity);
        await _db.SaveChangesAsync();

        _logger.LogInformation("TutorProfile created Id={Id} User={Email}", entity.Id, user.Email);
        TempData["ProfileCreateSuccess"] = true;
        return RedirectToAction(nameof(MyProfile));
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null)
            return RedirectToAction(nameof(Create));

        return View(MapToForm(profile, user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TutorProfileFormViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var profile = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null)
            return RedirectToAction(nameof(Create));

        // Owner-only: profile is loaded by current user Id
        ValidateBusinessRules(model);

        if (!ModelState.IsValid)
        {
            model.Id = profile.Id;
            model.IsVerified = profile.IsVerified;
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;
            return View(model);
        }

        profile.Headline = model.Headline.Trim();
        profile.Bio = model.Bio.Trim();
        profile.YearsOfExperience = model.YearsOfExperience;
        profile.EducationLevel = model.EducationLevel;
        profile.HourlyRate = model.HourlyRate;
        profile.TeachingMode = model.TeachingMode;
        profile.Address = NormalizeAddress(model);
        profile.IsAvailable = model.IsAvailable;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("TutorProfile updated Id={Id}", profile.Id);
        TempData["ProfileUpdateSuccess"] = true;
        return RedirectToAction(nameof(MyProfile));
    }

    private void ValidateBusinessRules(TutorProfileFormViewModel model)
    {
        if (!EducationLevels.Contains(model.EducationLevel))
            ModelState.AddModelError(nameof(model.EducationLevel), "Trình độ không hợp lệ");

        if (!TeachingModes.Contains(model.TeachingMode))
            ModelState.AddModelError(nameof(model.TeachingMode), "Hình thức dạy không hợp lệ");

        if (model.TeachingMode is "Offline" or "Both"
            && string.IsNullOrWhiteSpace(model.Address))
        {
            ModelState.AddModelError(nameof(model.Address),
                "Vui lòng nhập địa chỉ khi dạy Offline hoặc Cả hai");
        }
    }

    private static string? NormalizeAddress(TutorProfileFormViewModel model)
    {
        if (model.TeachingMode == "Online")
            return null;
        return string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
    }

    private static TutorProfileFormViewModel MapToForm(TutorProfile p, ApplicationUser user) => new()
    {
        Id = p.Id,
        Headline = p.Headline ?? "",
        Bio = p.Bio ?? "",
        YearsOfExperience = p.YearsOfExperience ?? 0,
        EducationLevel = p.EducationLevel ?? "",
        HourlyRate = p.HourlyRate ?? 0,
        TeachingMode = p.TeachingMode ?? "Online",
        Address = p.Address,
        IsAvailable = p.IsAvailable,
        IsVerified = p.IsVerified,
        AvatarUrl = user.AvatarUrl,
        FullName = user.FullName,
        Email = user.Email
    };
}
