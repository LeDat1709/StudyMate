using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyMate.Models;
using StudyMate.Services.Interfaces;
using StudyMate.ViewModels.Account;

namespace StudyMate.Controllers;

/// <summary>
/// Handles authentication and account management actions.
/// </summary>
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<AccountController> logger)
    {
        _userManager  = userManager;
        _signInManager = signInManager;
        _roleManager  = roleManager;
        _emailService = emailService;
        _otpService   = otpService;
        _logger       = logger;
    }

    // ── Register ─────────────────────────────────────────────────────────────

    /// <summary>Displays the registration form.</summary>
    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    /// <summary>Processes the registration form submission.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Only allow Student or Tutor roles via registration
        if (model.Role != "Student" && model.Role != "Tutor")
        {
            ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ");
            return View(model);
        }

        // Check duplicate email
        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng");
            return View(model);
        }

        // Create new user
        var user = new ApplicationUser
        {
            FullName         = model.FullName.Trim(),
            UserName         = model.Email.Trim().ToLower(),
            Email            = model.Email.Trim().ToLower(),
            IsActive         = true,
            IsEmailVerified  = false,
            EmailConfirmed   = false,
            CreatedAt        = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        // Assign role
        await _userManager.AddToRoleAsync(user, model.Role);

        // Generate OTP and send email
        var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id, "EmailVerify");

        var emailBody = $@"
            <div style='font-family:sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #e5e7eb;border-radius:8px'>
                <h2 style='color:#1d4ed8'>Xác thực email StudyMate</h2>
                <p>Xin chào <strong>{user.FullName}</strong>,</p>
                <p>Mã OTP xác thực email của bạn là:</p>
                <div style='font-size:2rem;font-weight:bold;letter-spacing:0.3rem;color:#1d4ed8;text-align:center;padding:16px 0'>{otpCode}</div>
                <p style='color:#6b7280'>Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này cho bất kỳ ai.</p>
                <hr style='border:none;border-top:1px solid #e5e7eb'/>
                <p style='color:#9ca3af;font-size:0.85rem'>StudyMate — Nền tảng kết nối gia sư</p>
            </div>";

        await _emailService.SendEmailAsync(
            to: user.Email,
            subject: "[StudyMate] Mã xác thực email của bạn",
            body: emailBody,
            isHtml: true);

        // Store email in TempData for VerifyEmail page
        TempData["RegisteredEmail"] = user.Email;

        _logger.LogInformation("New user registered: {Email}, Role: {Role}", user.Email, model.Role);

        return RedirectToAction(nameof(VerifyEmail));
    }

    // ── VerifyEmail placeholder ───────────────────────────────────────────────

    /// <summary>Placeholder — will be fully implemented in M1-T4.</summary>
    [HttpGet]
    public IActionResult VerifyEmail()
    {
        var email = TempData["RegisteredEmail"] as string;
        ViewBag.Email = email;
        return View();
    }
}
