using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using StudyMate.Services.Interfaces;

namespace StudyMate.Services.Implementations;

/// <summary>
/// Sends emails via Gmail SMTP using MailKit.
/// Configuration is read from the "Email" section in appsettings.json.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        var smtpHost    = _configuration["Email:SmtpHost"]    ?? "smtp.gmail.com";
        var smtpPort    = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var senderEmail = _configuration["Email:SenderEmail"] ?? string.Empty;
        var senderName  = _configuration["Email:SenderName"]  ?? "StudyMate";
        var password    = _configuration["Email:Password"]    ?? string.Empty;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart(isHtml ? "html" : "plain")
        {
            Text = body
        };

        try
        {
            using var client = new SmtpClient();

            // Connect with STARTTLS (port 587)
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(quit: true);

            _logger.LogInformation("Email sent successfully to {Recipient}", to);
        }
        catch (Exception ex)
        {
            // Log the error but do not throw — avoids crashing the app on email failure
            _logger.LogError(ex, "Failed to send email to {Recipient}. Subject: {Subject}", to, subject);
        }
    }
}
