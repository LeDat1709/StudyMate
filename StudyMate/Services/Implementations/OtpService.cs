using Microsoft.EntityFrameworkCore;
using StudyMate.Data;
using StudyMate.Models;
using StudyMate.Services.Interfaces;

namespace StudyMate.Services.Implementations;

/// <summary>
/// Handles OTP generation, validation, and invalidation.
/// Expiry durations are read from the "Otp" section in appsettings.json.
/// </summary>
public class OtpService : IOtpService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OtpService> _logger;

    private const int MaxFailedAttempts = 3;

    public OtpService(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<OtpService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GenerateAndSaveOtpAsync(string userId, string purpose)
    {
        // Invalidate all existing OTPs for this user + purpose
        await InvalidateAllOtpAsync(userId, purpose);

        // Generate a cryptographically random 6-digit code
        var code = Random.Shared.Next(100_000, 999_999).ToString();

        // Determine expiry based on purpose
        var expiryMinutes = purpose == "ForgotPassword"
            ? int.Parse(_configuration["Otp:ResetPasswordExpiryMinutes"] ?? "10")
            : int.Parse(_configuration["Otp:ExpiryMinutes"] ?? "5");

        var otp = new OtpCode
        {
            UserId     = userId,
            Code       = code,
            Purpose    = purpose,
            ExpiredAt  = DateTime.UtcNow.AddMinutes(expiryMinutes),
            IsUsed     = false,
            FailedAttempts = 0,
            CreatedAt  = DateTime.UtcNow
        };

        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP generated for user {UserId}, purpose {Purpose}", userId, purpose);

        return code;
    }

    /// <inheritdoc />
    public async Task<OtpValidationResult> ValidateOtpAsync(string userId, string code, string purpose)
    {
        // Get the most recent unused OTP for this user + purpose
        var otp = await _context.OtpCodes
            .Where(x => x.UserId == userId && x.Purpose == purpose && !x.IsUsed)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp is null)
        {
            _logger.LogWarning("No active OTP found for user {UserId}, purpose {Purpose}", userId, purpose);
            return OtpValidationResult.Invalid;
        }

        // Check failed attempts before anything else
        if (otp.FailedAttempts >= MaxFailedAttempts)
        {
            _logger.LogWarning("OTP locked due to too many failed attempts for user {UserId}", userId);
            return OtpValidationResult.TooManyAttempts;
        }

        // Check expiry
        if (otp.ExpiredAt < DateTime.UtcNow)
        {
            _logger.LogInformation("OTP expired for user {UserId}, purpose {Purpose}", userId, purpose);
            return OtpValidationResult.Expired;
        }

        // Check if already used (defensive — should not reach here due to query filter)
        if (otp.IsUsed)
        {
            return OtpValidationResult.AlreadyUsed;
        }

        // Check code match
        if (otp.Code != code)
        {
            otp.FailedAttempts++;
            await _context.SaveChangesAsync();

            _logger.LogWarning("Invalid OTP code for user {UserId}. Attempts: {Attempts}", userId, otp.FailedAttempts);

            // Return TooManyAttempts immediately if threshold just reached
            return otp.FailedAttempts >= MaxFailedAttempts
                ? OtpValidationResult.TooManyAttempts
                : OtpValidationResult.Invalid;
        }

        // Valid — mark as used
        otp.IsUsed = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("OTP validated successfully for user {UserId}, purpose {Purpose}", userId, purpose);

        return OtpValidationResult.Valid;
    }

    /// <inheritdoc />
    public async Task InvalidateAllOtpAsync(string userId, string purpose)
    {
        var otps = await _context.OtpCodes
            .Where(x => x.UserId == userId && x.Purpose == purpose && !x.IsUsed)
            .ToListAsync();

        if (otps.Count == 0) return;

        foreach (var otp in otps)
            otp.IsUsed = true;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Invalidated {Count} OTP(s) for user {UserId}, purpose {Purpose}",
            otps.Count, userId, purpose);
    }
}
