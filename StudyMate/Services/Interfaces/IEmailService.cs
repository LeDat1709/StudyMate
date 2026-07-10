namespace StudyMate.Services.Interfaces;

/// <summary>
/// Defines the contract for sending emails in the StudyMate application.
/// Used for OTP verification emails and password reset emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously via SMTP.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">Email body content (plain text or HTML).</param>
    /// <param name="isHtml">Set to true if body contains HTML markup. Defaults to false.</param>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
}
