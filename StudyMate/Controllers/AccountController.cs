using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudyMate.Models;
using StudyMate.Services.Interfaces;
using StudyMate.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;

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
    private readonly IWebHostEnvironment _env;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IEmailService emailService,
        IOtpService otpService,
        ILogger<AccountController> logger,
        IWebHostEnvironment env)
    {
        _userManager   = userManager;
        _signInManager = signInManager;
        _roleManager   = roleManager;
        _emailService  = emailService;
        _otpService    = otpService;
        _logger        = logger;
        _env           = env;
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

    // ── Profile / Avatar ───────────────────────────────────────────────────

    /// <summary>Displays the profile page for the currently authenticated user.</summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var vm = new ProfileViewModel
        {
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl
        };

        ViewData["ProfileUpdateSuccess"] = TempData["ProfileUpdateSuccess"];
        return View(vm);
    }

    /// <summary>Updates profile fields (FullName, PhoneNumber).</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        var res = await _userManager.UpdateAsync(user);
        if (!res.Succeeded)
        {
            foreach (var e in res.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        TempData["ProfileUpdateSuccess"] = true;
        return RedirectToAction(nameof(Profile));
    }

    /// <summary>Uploads avatar file via AJAX and returns the new image URL.</summary>
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file == null)
            return BadRequest(new { error = "Không có file được gửi." });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(new { error = "Định dạng không được hỗ trợ. Chỉ JPG, PNG, WEBP." });

        const long MaxBytes = 2 * 1024 * 1024; // 2MB
        if (file.Length > MaxBytes)
            return BadRequest(new { error = "Kích thước file vượt quá 2MB." });

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var safeName = Path.GetFileNameWithoutExtension(file.FileName);
        var filename = $"{user.Id}_{timestamp}{ext}";
        var fullPath = Path.Combine(uploadsDir, filename);

        await using (var fs = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }

        user.AvatarUrl = $"/uploads/avatars/{filename}";
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Json(new { success = true, url = user.AvatarUrl });
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    /// <summary>Displays the login form.</summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already authenticated — redirect away
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    /// <summary>Processes login form submission.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Do not reveal whether email exists or not
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ admin để được hỗ trợ.");
            return View(model);
        }

        if (!user.EmailConfirmed)
        {
            TempData["RegisteredEmail"] = user.Email;
            ModelState.AddModelError(string.Empty, "Vui lòng xác thực email trước khi đăng nhập.");
            ViewData["ShowResendLink"] = true;
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in: {Email}", user.Email);
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out: {Email}", user.Email);
            ModelState.AddModelError(string.Empty, "Tài khoản tạm khóa do nhập sai quá nhiều lần. Vui lòng thử lại sau.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác");
        return View(model);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    /// <summary>Signs the user out and redirects to home.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User signed out");
        return RedirectToAction("Index", "Home");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    // ── ForgotPassword ────────────────────────────────────────────────────────

    /// <summary>Displays the forgot password form (step 1 — enter email).</summary>
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    /// <summary>Sends OTP to the provided email if it exists.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Always show same message — do not reveal whether email exists
        if (user != null && user.EmailConfirmed)
        {
            var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id, "ForgotPassword");

            var emailBody = $@"
                <div style='font-family:sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #e5e7eb;border-radius:8px'>
                    <h2 style='color:#1d4ed8'>Đặt lại mật khẩu StudyMate</h2>
                    <p>Xin chào <strong>{user.FullName}</strong>,</p>
                    <p>Mã OTP để đặt lại mật khẩu của bạn là:</p>
                    <div style='font-size:2rem;font-weight:bold;letter-spacing:0.3rem;color:#1d4ed8;text-align:center;padding:16px 0'>{otpCode}</div>
                    <p style='color:#6b7280'>Mã có hiệu lực trong <strong>10 phút</strong>. Không chia sẻ mã này cho bất kỳ ai.</p>
                    <hr style='border:none;border-top:1px solid #e5e7eb'/>
                    <p style='color:#9ca3af;font-size:0.85rem'>StudyMate — Nền tảng kết nối gia sư</p>
                </div>";

            await _emailService.SendEmailAsync(
                to: user.Email!,
                subject: "[StudyMate] Mã đặt lại mật khẩu của bạn",
                body: emailBody,
                isHtml: true);

            _logger.LogInformation("Password reset OTP sent to: {Email}", user.Email);
        }

        TempData["ForgotPasswordEmail"] = model.Email;
        return RedirectToAction(nameof(VerifyResetOtp));
    }

    // ── VerifyResetOtp ────────────────────────────────────────────────────────

    /// <summary>Displays the OTP verification form for password reset (step 2).</summary>
    [HttpGet]
    public IActionResult VerifyResetOtp()
    {
        var email = TempData["ForgotPasswordEmail"] as string;
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(ForgotPassword));

        return View(new VerifyOtpViewModel { Email = email });
    }

    /// <summary>Validates the reset OTP and redirects to ResetPassword on success.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyResetOtp(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ForgotPasswordEmail"] = model.Email;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return RedirectToAction(nameof(ForgotPassword));

        var result = await _otpService.ValidateOtpAsync(user.Id, model.Code, "ForgotPassword");

        switch (result)
        {
            case OtpValidationResult.Valid:
                TempData["ResetPasswordEmail"] = model.Email;
                return RedirectToAction(nameof(ResetPassword));

            case OtpValidationResult.Expired:
                ModelState.AddModelError(nameof(model.Code), "Mã OTP đã hết hạn. Vui lòng yêu cầu lại.");
                break;

            case OtpValidationResult.TooManyAttempts:
                ModelState.AddModelError(nameof(model.Code), "Bạn đã nhập sai quá nhiều lần. Vui lòng yêu cầu lại OTP mới.");
                break;

            default:
                ModelState.AddModelError(nameof(model.Code), "Mã OTP không chính xác");
                break;
        }

        TempData["ForgotPasswordEmail"] = model.Email;
        return View(model);
    }

    // ── ResetPassword ─────────────────────────────────────────────────────────

    /// <summary>Displays the new password form (step 3).</summary>
    [HttpGet]
    public IActionResult ResetPassword()
    {
        var email = TempData["ResetPasswordEmail"] as string;
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(ForgotPassword));

        return View(new ResetPasswordViewModel { Email = email });
    }

    /// <summary>Updates the user's password and signs them in.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ResetPasswordEmail"] = model.Email;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return RedirectToAction(nameof(ForgotPassword));

        // Remove old password and set new one
        var token  = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            TempData["ResetPasswordEmail"] = model.Email;
            return View(model);
        }

        // Invalidate all existing sessions
        await _userManager.UpdateSecurityStampAsync(user);

        // Sign in with new password
        await _signInManager.SignOutAsync();
        await _signInManager.SignInAsync(user, isPersistent: false);

        _logger.LogInformation("Password reset successfully for: {Email}", user.Email);

        TempData["SuccessMessage"] = "Đặt lại mật khẩu thành công!";
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Displays the OTP verification form.</summary>
    [HttpGet]
    public IActionResult VerifyEmail()
    {
        var email = TempData["RegisteredEmail"] as string;

        // If accessed directly without going through Register, redirect back
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Register));

        return View(new VerifyOtpViewModel { Email = email });
    }

    /// <summary>Processes OTP submission and verifies the user's email.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyEmail(VerifyOtpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["RegisteredEmail"] = model.Email;
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Không tìm thấy tài khoản");
            TempData["RegisteredEmail"] = model.Email;
            return View(model);
        }

        var result = await _otpService.ValidateOtpAsync(user.Id, model.Code, "EmailVerify");

        switch (result)
        {
            case OtpValidationResult.Valid:
                user.EmailConfirmed   = true;
                user.IsEmailVerified  = true;
                user.UpdatedAt        = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("Email verified and signed in: {Email}", user.Email);
                return RedirectToAction("Index", "Home");

            case OtpValidationResult.Expired:
                ModelState.AddModelError(nameof(model.Code), "Mã OTP đã hết hạn. Vui lòng nhấn \"Gửi lại OTP\".");
                break;

            case OtpValidationResult.AlreadyUsed:
                ModelState.AddModelError(nameof(model.Code), "Mã OTP đã được sử dụng. Vui lòng nhấn \"Gửi lại OTP\".");
                break;

            case OtpValidationResult.TooManyAttempts:
                ModelState.AddModelError(nameof(model.Code), "Bạn đã nhập sai quá nhiều lần. Vui lòng nhấn \"Gửi lại OTP\" để lấy mã mới.");
                break;

            default: // Invalid
                ModelState.AddModelError(nameof(model.Code), "Mã OTP không chính xác");
                break;
        }

        TempData["RegisteredEmail"] = model.Email;
        return View(model);
    }

    /// <summary>Invalidates old OTP and sends a new one to the user's email.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendOtp(string email)
    {
        if (string.IsNullOrEmpty(email))
            return RedirectToAction(nameof(Register));

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return RedirectToAction(nameof(Register));

        // Already verified — no need to resend
        if (user.EmailConfirmed)
            return RedirectToAction("Index", "Home");

        var otpCode = await _otpService.GenerateAndSaveOtpAsync(user.Id, "EmailVerify");

        var emailBody = $@"
            <div style='font-family:sans-serif;max-width:480px;margin:auto;padding:24px;border:1px solid #e5e7eb;border-radius:8px'>
                <h2 style='color:#1d4ed8'>Xác thực email StudyMate</h2>
                <p>Xin chào <strong>{user.FullName}</strong>,</p>
                <p>Mã OTP xác thực email mới của bạn là:</p>
                <div style='font-size:2rem;font-weight:bold;letter-spacing:0.3rem;color:#1d4ed8;text-align:center;padding:16px 0'>{otpCode}</div>
                <p style='color:#6b7280'>Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này cho bất kỳ ai.</p>
                <hr style='border:none;border-top:1px solid #e5e7eb'/>
                <p style='color:#9ca3af;font-size:0.85rem'>StudyMate — Nền tảng kết nối gia sư</p>
            </div>";

        await _emailService.SendEmailAsync(
            to: user.Email!,
            subject: "[StudyMate] Mã xác thực email mới của bạn",
            body: emailBody,
            isHtml: true);

        TempData["RegisteredEmail"] = email;
        TempData["ResendSuccess"]   = true;

        _logger.LogInformation("OTP resent to: {Email}", email);

        return RedirectToAction(nameof(VerifyEmail));
    }
}
