namespace StudyMate.Services.Interfaces;

/// <summary>
/// Result of an OTP validation attempt.
/// </summary>
public enum OtpValidationResult
{
    /// <summary>OTP is correct and valid.</summary>
    Valid,

    /// <summary>OTP code does not match.</summary>
    Invalid,

    /// <summary>OTP has expired.</summary>
    Expired,

    /// <summary>OTP has already been used.</summary>
    AlreadyUsed,

    /// <summary>Too many failed attempts (>= 3). OTP is locked.</summary>
    TooManyAttempts
}
