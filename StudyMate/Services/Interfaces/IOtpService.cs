namespace StudyMate.Services.Interfaces;

/// <summary>
/// Defines the contract for OTP generation and validation.
/// Used for email verification (EmailVerify) and password reset (ForgotPassword).
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a 6-digit OTP, invalidates any existing OTP for the same purpose,
    /// saves the new OTP to the database, and returns the code.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="purpose">Purpose identifier: "EmailVerify" or "ForgotPassword".</param>
    /// <returns>The generated 6-digit OTP code as a string.</returns>
    Task<string> GenerateAndSaveOtpAsync(string userId, string purpose);

    /// <summary>
    /// Validates a submitted OTP code against the stored record.
    /// Increments FailedAttempts on mismatch and marks as used on success.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="code">The OTP code submitted by the user.</param>
    /// <param name="purpose">Purpose identifier: "EmailVerify" or "ForgotPassword".</param>
    /// <returns>An <see cref="OtpValidationResult"/> indicating the outcome.</returns>
    Task<OtpValidationResult> ValidateOtpAsync(string userId, string code, string purpose);

    /// <summary>
    /// Marks all unused OTPs for the given user and purpose as used (invalidated).
    /// Called before generating a new OTP to prevent reuse of old codes.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <param name="purpose">Purpose identifier: "EmailVerify" or "ForgotPassword".</param>
    Task InvalidateAllOtpAsync(string userId, string purpose);
}
