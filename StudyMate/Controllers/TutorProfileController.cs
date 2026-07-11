using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;
using StudyMate.ViewModels.Account;
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
    private static readonly string[] CertificateAllowedExt = [".jpg", ".jpeg", ".png", ".pdf"];
    private const long CertificateMaxBytes = 10L * 1024 * 1024;

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFileStorageService _files;
    private readonly IContentModerationService _moderation;
    private readonly ILogger<TutorProfileController> _logger;

    public static readonly string[] EducationLevels =
        ["Cử nhân", "Thạc sĩ", "Tiến sĩ", "Khác"];

    public static readonly string[] TeachingModes =
        ["Online", "Offline", "Both"];

    public TutorProfileController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> userManager,
        IFileStorageService files,
        IContentModerationService moderation,
        ILogger<TutorProfileController> logger)
    {
        _db = db;
        _userManager = userManager;
        _files = files;
        _moderation = moderation;
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

    // ── M2-T7 Certificates ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Certificates()
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        var vm = await BuildCertificatesPageAsync(profile.Id);
        ViewData["UploadSuccess"] = TempData["CertificateUploadSuccess"];
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadCertificate(
        [Bind(Prefix = "Upload")] UploadCertificateViewModel model)
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        ModelState.Clear();
        if (string.IsNullOrWhiteSpace(model.Title))
            ModelState.AddModelError("Upload.Title", "Vui lòng nhập tên chứng chỉ");
        else if (model.Title.Length > 200)
            ModelState.AddModelError("Upload.Title", "Tên chứng chỉ tối đa 200 ký tự");

        if (model.File == null || model.File.Length == 0)
            ModelState.AddModelError("Upload.File", "Vui lòng chọn file chứng chỉ");

        if (!ModelState.IsValid)
            return View("Certificates", await BuildCertificatesPageAsync(profile.Id, model));

        string fileUrl;
        try
        {
            fileUrl = await _files.SaveAsync(model.File!, "certificates", CertificateAllowedExt, CertificateMaxBytes);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Upload.File", ex.Message);
            return View("Certificates", await BuildCertificatesPageAsync(profile.Id, model));
        }

        var title = model.Title.Trim();
        string? aiNote = null;
        try
        {
            aiNote = await _moderation.ReviewCertificateAsync(fileUrl, title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Moderation failed for certificate {Title}", title);
            aiNote = "AI review unavailable";
        }

        _db.TutorCertificates.Add(new TutorCertificate
        {
            TutorProfileId = profile.Id,
            Title = title,
            IssuedBy = string.IsNullOrWhiteSpace(model.IssuedBy) ? null : model.IssuedBy.Trim(),
            IssuedDate = model.IssuedDate,
            CertType = string.IsNullOrWhiteSpace(model.CertType) ? null : model.CertType.Trim(),
            FileUrl = fileUrl,
            IsVerified = false,
            AiVerifyNote = aiNote,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["CertificateUploadSuccess"] = true;
        return RedirectToAction(nameof(Certificates));
    }

    private async Task<CertificatesPageViewModel> BuildCertificatesPageAsync(
        int profileId, UploadCertificateViewModel? upload = null)
    {
        var items = await _db.TutorCertificates.AsNoTracking()
            .Where(c => c.TutorProfileId == profileId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CertificateItemViewModel
            {
                Id = c.Id,
                Title = c.Title,
                IssuedBy = c.IssuedBy,
                IssuedDate = c.IssuedDate,
                FileUrl = c.FileUrl,
                CertType = c.CertType,
                IsVerified = c.IsVerified,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new CertificatesPageViewModel
        {
            Certificates = items,
            Upload = upload ?? new UploadCertificateViewModel()
        };
    }

    // ── M2-T8 Availability ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Availability()
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        var slots = await _db.TutorAvailabilities.AsNoTracking()
            .Where(a => a.TutorProfileId == profile.Id)
            .OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime)
            .ToListAsync();

        return View(new AvailabilityPageViewModel
        {
            Items = slots.Select(s => new AvailabilityItemViewModel
            {
                Id = s.Id,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }).ToList(),
            Form = new AvailabilityFormViewModel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAvailability(AvailabilityFormViewModel form)
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        if (form.DayOfWeek is < 0 or > 6)
            ModelState.AddModelError(nameof(form.DayOfWeek), "Ngày trong tuần không hợp lệ");
        if (form.StartTime >= form.EndTime)
            ModelState.AddModelError(nameof(form.EndTime), "Giờ bắt đầu phải trước giờ kết thúc");

        if (!ModelState.IsValid)
        {
            var slots = await _db.TutorAvailabilities.AsNoTracking()
                .Where(a => a.TutorProfileId == profile.Id)
                .OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime)
                .ToListAsync();
            return View("Availability", new AvailabilityPageViewModel
            {
                Items = slots.Select(s => new AvailabilityItemViewModel
                {
                    Id = s.Id,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList(),
                Form = form
            });
        }

        _db.TutorAvailabilities.Add(new TutorAvailability
        {
            TutorProfileId = profile.Id,
            DayOfWeek = form.DayOfWeek,
            StartTime = form.StartTime,
            EndTime = form.EndTime
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Availability));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAvailability(int id)
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        var slot = await _db.TutorAvailabilities
            .FirstOrDefaultAsync(a => a.Id == id && a.TutorProfileId == profile.Id);
        if (slot != null)
        {
            _db.TutorAvailabilities.Remove(slot);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Availability));
    }

    // ── M2-T9 Demo Lessons ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> DemoLessons()
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        var list = await _db.DemoLessons.AsNoTracking()
            .Where(d => d.TutorProfileId == profile.Id)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return View(new DemoLessonListViewModel
        {
            Items = list.Select(d => new DemoLessonItemViewModel
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                VideoUrl = d.VideoUrl,
                CreatedAt = d.CreatedAt
            }).ToList(),
            Form = new DemoLessonFormViewModel()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDemoLesson(DemoLessonFormViewModel form)
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        if (string.IsNullOrWhiteSpace(form.Title))
            ModelState.AddModelError(nameof(form.Title), "Vui lòng nhập tiêu đề");

        if (!ModelState.IsValid)
            return RedirectToAction(nameof(DemoLessons));

        _db.DemoLessons.Add(new DemoLesson
        {
            TutorProfileId = profile.Id,
            Title = form.Title.Trim(),
            Description = form.Description?.Trim(),
            VideoUrl = form.VideoUrl?.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(DemoLessons));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDemoLesson(int id)
    {
        var profile = await RequireProfileAsync();
        if (profile == null)
            return RedirectToAction(nameof(Create));

        var demo = await _db.DemoLessons
            .FirstOrDefaultAsync(d => d.Id == id && d.TutorProfileId == profile.Id);
        if (demo != null)
        {
            _db.DemoLessons.Remove(demo);
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(DemoLessons));
    }

    // ── M2-T10 Public profile ────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var profile = await _db.TutorProfiles.AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.TutorSubjects).ThenInclude(ts => ts.Subject)
            .Include(p => p.Certificates)
            .Include(p => p.Availabilities)
            .Include(p => p.DemoLessons)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (profile == null)
            return NotFound();

        var isOwner = User.Identity?.IsAuthenticated == true
            && profile.UserId == _userManager.GetUserId(User);

        if (!profile.IsVerified && !isOwner && !User.IsInRole("Admin"))
            return NotFound();

        var vm = new PublicTutorProfileViewModel
        {
            Id = profile.Id,
            FullName = profile.User?.FullName ?? "",
            AvatarUrl = profile.User?.AvatarUrl,
            Headline = profile.Headline,
            Bio = profile.Bio,
            VideoIntroUrl = profile.VideoIntroUrl,
            YearsOfExperience = profile.YearsOfExperience,
            EducationLevel = profile.EducationLevel,
            HourlyRate = profile.HourlyRate,
            TeachingMode = profile.TeachingMode,
            Address = profile.Address,
            AverageRating = profile.AverageRating,
            TotalReviews = profile.TotalReviews,
            IsVerified = profile.IsVerified,
            IsAvailable = profile.IsAvailable,
            Subjects = profile.TutorSubjects
                .Select(ts => ts.Subject?.Name ?? "")
                .Where(n => n.Length > 0)
                .ToList(),
            Certificates = profile.Certificates
                .Where(c => c.IsVerified)
                .Select(c => new PublicCertViewModel
                {
                    Title = c.Title,
                    CertType = c.CertType,
                    IssuedBy = c.IssuedBy
                }).ToList(),
            Availabilities = profile.Availabilities
                .OrderBy(a => a.DayOfWeek).ThenBy(a => a.StartTime)
                .Select(a => new AvailabilityItemViewModel
                {
                    Id = a.Id,
                    DayOfWeek = a.DayOfWeek,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime
                }).ToList(),
            DemoLessons = profile.DemoLessons
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DemoLessonItemViewModel
                {
                    Id = d.Id,
                    Title = d.Title,
                    Description = d.Description,
                    VideoUrl = d.VideoUrl,
                    CreatedAt = d.CreatedAt
                }).ToList(),
            ShowContactButton = User.Identity?.IsAuthenticated == true && User.IsInRole("Student")
        };

        return View(vm);
    }

    // ── M2-T11 Search ────────────────────────────────────────────────────────

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Search(
        int? subjectId,
        decimal? minRate,
        decimal? maxRate,
        string? teachingMode,
        string? educationLevel,
        decimal? minRating,
        string? q,
        string sort = "Newest",
        int page = 1)
    {
        const int pageSize = 12;
        if (page < 1) page = 1;

        var query = _db.TutorProfiles.AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.TutorSubjects).ThenInclude(ts => ts.Subject)
            .Where(p => p.IsVerified);

        if (subjectId is > 0)
            query = query.Where(p => p.TutorSubjects.Any(ts => ts.SubjectId == subjectId));
        if (minRate is not null)
            query = query.Where(p => p.HourlyRate >= minRate);
        if (maxRate is not null)
            query = query.Where(p => p.HourlyRate <= maxRate);
        if (!string.IsNullOrWhiteSpace(teachingMode))
            query = query.Where(p => p.TeachingMode == teachingMode || p.TeachingMode == "Both");
        if (!string.IsNullOrWhiteSpace(educationLevel))
            query = query.Where(p => p.EducationLevel == educationLevel);
        if (minRating is not null)
            query = query.Where(p => p.AverageRating >= minRating);
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                (p.Headline != null && p.Headline.Contains(term))
                || (p.Bio != null && p.Bio.Contains(term))
                || (p.User != null && p.User.FullName.Contains(term)));
        }

        query = sort switch
        {
            "RatingDesc" => query.OrderByDescending(p => p.AverageRating),
            "PriceAsc" => query.OrderBy(p => p.HourlyRate),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new TutorSearchResultItem
            {
                Id = p.Id,
                FullName = p.User != null ? p.User.FullName : "",
                AvatarUrl = p.User != null ? p.User.AvatarUrl : null,
                Headline = p.Headline,
                HourlyRate = p.HourlyRate,
                TeachingMode = p.TeachingMode,
                EducationLevel = p.EducationLevel,
                AverageRating = p.AverageRating,
                TotalReviews = p.TotalReviews,
                Subjects = p.TutorSubjects.Select(ts => ts.Subject!.Name).ToList()
            })
            .ToListAsync();

        var subjects = await _db.Subjects.AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(new TutorSearchViewModel
        {
            SubjectId = subjectId,
            MinRate = minRate,
            MaxRate = maxRate,
            TeachingMode = teachingMode,
            EducationLevel = educationLevel,
            MinRating = minRating,
            Keyword = q,
            Sort = sort,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            Results = items,
            Subjects = subjects.Select(s => new SubjectOptionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Category = s.Category
            }).ToList()
        });
    }

    private async Task<TutorProfile?> RequireProfileAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return null;
        return await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
    }
}
