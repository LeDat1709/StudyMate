using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;
using StudyMate.ViewModels.TutorProfile;

namespace StudyMate.Controllers;

/// <summary>
/// Quản lý hồ sơ gia sư (M2).
/// </summary>
[Authorize(Roles = "Tutor")]
public class TutorProfileController : Controller
{
    private static readonly string[] VideoAllowedExt = [".mp4"];
    private const long VideoMaxBytes = 50L * 1024 * 1024;

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _files;
    private readonly ILogger<TutorProfileController> _logger;

    public static readonly string[] EducationLevels =
        ["Cử nhân", "Thạc sĩ", "Tiến sĩ", "Khác"];

    public static readonly string[] TeachingModes =
        ["Online", "Offline", "Both"];

    public TutorProfileController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IFileStorageService files,
        ILogger<TutorProfileController> logger)
    {
        _db = db;
        _userManager = userManager;
        _files = files;
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
            .Include(p => p.TutorSubjects)
            .ThenInclude(ts => ts.Subject)
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        if (profile == null)
            return RedirectToAction(nameof(Create));

        var vm = await MapToFormAsync(profile, user);
        ViewData["UpdateSuccess"] = TempData["ProfileUpdateSuccess"];
        ViewData["CreateSuccess"] = TempData["ProfileCreateSuccess"];
        ViewData["SubjectNames"] = profile.TutorSubjects
            .Select(ts => ts.Subject?.Name)
            .Where(n => n != null)
            .ToList();
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

        var vm = new TutorProfileFormViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            IsAvailable = true,
            TeachingMode = "Online"
        };
        await PopulateSubjectsAsync(vm);
        return View(vm);
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
        ValidateSubjects(model);

        if (!ModelState.IsValid)
        {
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;
            await PopulateSubjectsAsync(model);
            return View(model);
        }

        string? videoUrl = null;
        try
        {
            videoUrl = await ResolveVideoUrlAsync(model, previousUrl: null);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.VideoFile), ex.Message);
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;
            await PopulateSubjectsAsync(model);
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
            VideoIntroUrl = videoUrl,
            IsAvailable = model.IsAvailable,
            IsVerified = false,
            AverageRating = 0,
            TotalReviews = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.TutorProfiles.Add(entity);
        await _db.SaveChangesAsync();

        await SyncSubjectsAsync(entity.Id, model.SelectedSubjectIds);

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

        var profile = await _db.TutorProfiles
            .Include(p => p.TutorSubjects)
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null)
            return RedirectToAction(nameof(Create));

        return View(await MapToFormAsync(profile, user));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TutorProfileFormViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var profile = await _db.TutorProfiles
            .Include(p => p.TutorSubjects)
            .FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null)
            return RedirectToAction(nameof(Create));

        ValidateBusinessRules(model);
        ValidateSubjects(model);

        if (!ModelState.IsValid)
        {
            model.Id = profile.Id;
            model.IsVerified = profile.IsVerified;
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;
            model.VideoIntroUrl = profile.VideoIntroUrl;
            await PopulateSubjectsAsync(model);
            return View(model);
        }

        string? videoUrl;
        try
        {
            videoUrl = await ResolveVideoUrlAsync(model, profile.VideoIntroUrl);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.VideoFile), ex.Message);
            model.Id = profile.Id;
            model.IsVerified = profile.IsVerified;
            model.FullName = user.FullName;
            model.Email = user.Email;
            model.AvatarUrl = user.AvatarUrl;
            await PopulateSubjectsAsync(model);
            return View(model);
        }

        profile.Headline = model.Headline.Trim();
        profile.Bio = model.Bio.Trim();
        profile.YearsOfExperience = model.YearsOfExperience;
        profile.EducationLevel = model.EducationLevel;
        profile.HourlyRate = model.HourlyRate;
        profile.TeachingMode = model.TeachingMode;
        profile.Address = NormalizeAddress(model);
        profile.VideoIntroUrl = videoUrl;
        profile.IsAvailable = model.IsAvailable;
        profile.UpdatedAt = DateTime.UtcNow;

        await SyncSubjectsAsync(profile.Id, model.SelectedSubjectIds);
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

        if (!string.IsNullOrWhiteSpace(model.VideoIntroUrl)
            && model.VideoFile == null
            && !IsAllowedVideoUrl(model.VideoIntroUrl))
        {
            ModelState.AddModelError(nameof(model.VideoIntroUrl),
                "URL video phải là YouTube, Vimeo hoặc đường dẫn /uploads/");
        }
    }

    private void ValidateSubjects(TutorProfileFormViewModel model)
    {
        model.SelectedSubjectIds ??= new List<int>();
        var distinct = model.SelectedSubjectIds.Distinct().ToList();
        model.SelectedSubjectIds = distinct;

        if (distinct.Count < 1)
            ModelState.AddModelError(nameof(model.SelectedSubjectIds), "Chọn tối thiểu 1 môn dạy");
        else if (distinct.Count > 10)
            ModelState.AddModelError(nameof(model.SelectedSubjectIds), "Chọn tối đa 10 môn dạy");
    }

    private async Task PopulateSubjectsAsync(TutorProfileFormViewModel model)
    {
        var selected = new HashSet<int>(model.SelectedSubjectIds ?? []);
        model.AvailableSubjects = await _db.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .Select(s => new SubjectOptionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category,
                Selected = selected.Contains(s.Id)
            })
            .ToListAsync();
    }

    private async Task SyncSubjectsAsync(int profileId, List<int> selectedIds)
    {
        var validIds = await _db.Subjects
            .Where(s => selectedIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();

        var existing = await _db.TutorSubjects
            .Where(ts => ts.TutorProfileId == profileId)
            .ToListAsync();

        _db.TutorSubjects.RemoveRange(existing);

        foreach (var sid in validIds.Distinct())
        {
            _db.TutorSubjects.Add(new TutorSubject
            {
                TutorProfileId = profileId,
                SubjectId = sid
            });
        }

        await _db.SaveChangesAsync();
    }

    private async Task<string?> ResolveVideoUrlAsync(TutorProfileFormViewModel model, string? previousUrl)
    {
        if (model.VideoFile != null && model.VideoFile.Length > 0)
        {
            var url = await _files.SaveAsync(model.VideoFile, "tutor-videos", VideoAllowedExt, VideoMaxBytes);
            if (!string.IsNullOrEmpty(previousUrl)
                && previousUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                await _files.DeleteIfExistsAsync(previousUrl);
            }
            return url;
        }

        if (!string.IsNullOrWhiteSpace(model.VideoIntroUrl))
        {
            var trimmed = model.VideoIntroUrl.Trim();
            if (!IsAllowedVideoUrl(trimmed))
                throw new InvalidOperationException("URL video không hợp lệ.");

            // Replacing local upload with external URL → delete old local
            if (!string.IsNullOrEmpty(previousUrl)
                && previousUrl.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(previousUrl, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                await _files.DeleteIfExistsAsync(previousUrl);
            }

            return trimmed;
        }

        return previousUrl;
    }

    private static bool IsAllowedVideoUrl(string url)
    {
        if (url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        var host = uri.Host.ToLowerInvariant();
        return host.Contains("youtube.com")
               || host.Contains("youtu.be")
               || host.Contains("vimeo.com");
    }

    private static string? NormalizeAddress(TutorProfileFormViewModel model)
    {
        if (model.TeachingMode == "Online")
            return null;
        return string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
    }

    private async Task<TutorProfileFormViewModel> MapToFormAsync(TutorProfile p, ApplicationUser user)
    {
        var vm = new TutorProfileFormViewModel
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
            Email = user.Email,
            VideoIntroUrl = p.VideoIntroUrl,
            SelectedSubjectIds = p.TutorSubjects?.Select(ts => ts.SubjectId).ToList() ?? new List<int>()
        };
        await PopulateSubjectsAsync(vm);
        return vm;
    }

    /// <summary>Embed helper for YouTube/Vimeo (used by views via static).</summary>
    public static string? ToEmbedUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        if (url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            return null; // use HTML5 video

        // youtu.be/ID or youtube.com/watch?v=ID
        var yt = Regex.Match(url, @"(?:youtu\.be/|youtube\.com/watch\?v=)([A-Za-z0-9_-]{6,})");
        if (yt.Success)
            return $"https://www.youtube.com/embed/{yt.Groups[1].Value}";

        var vm = Regex.Match(url, @"vimeo\.com/(\d+)");
        if (vm.Success)
            return $"https://player.vimeo.com/video/{vm.Groups[1].Value}";

        return null;
    }
}
