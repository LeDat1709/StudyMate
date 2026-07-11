using StudyMate.Services.Interfaces;

namespace StudyMate.Services.Implementations;

/// <summary>
/// Stub kiểm duyệt — ghi note pending; thay bằng Python API sau.
/// </summary>
public class ContentModerationService : IContentModerationService
{
    private readonly ILogger<ContentModerationService> _logger;

    public ContentModerationService(ILogger<ContentModerationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<string> ReviewCertificateAsync(string? fileUrl, string title)
    {
        _logger.LogInformation("AI moderation stub for cert '{Title}' file={File}", title, fileUrl);
        return Task.FromResult("Pending AI review (stub)");
    }
}
