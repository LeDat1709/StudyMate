using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using StudyMate.Services.Interfaces;

namespace StudyMate.Services.Implementations;

/// <summary>
/// Lưu file vào wwwroot/uploads/{subFolder}/.
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> SaveAsync(
        IFormFile file,
        string subFolder,
        string[] allowedExtensions,
        long maxBytes)
    {
        Validate(file, allowedExtensions, maxBytes);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var dir = EnsureUploadDir(subFolder);
        var filename = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, filename);

        await using (var fs = File.Create(fullPath))
        {
            await file.CopyToAsync(fs);
        }

        return ToRelativeUrl(subFolder, filename);
    }

    /// <inheritdoc />
    public async Task<string> SaveImageResizedAsync(
        IFormFile file,
        string subFolder,
        string[] allowedExtensions,
        long maxBytes,
        int width,
        int height)
    {
        Validate(file, allowedExtensions, maxBytes);

        var dir = EnsureUploadDir(subFolder);
        // Always store as .jpg after resize for consistent size/format
        var filename = $"{Guid.NewGuid():N}.jpg";
        var fullPath = Path.Combine(dir, filename);

        await using var input = file.OpenReadStream();
        using var image = await Image.LoadAsync(input);

        image.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = ResizeMode.Crop,
            Position = AnchorPositionMode.Center
        }));

        await image.SaveAsJpegAsync(fullPath, new JpegEncoder { Quality = 85 });

        return ToRelativeUrl(subFolder, filename);
    }

    /// <inheritdoc />
    public Task DeleteIfExistsAsync(string? relativeUrl)
    {
        if (string.IsNullOrWhiteSpace(relativeUrl))
            return Task.CompletedTask;

        // Only allow deleting under /uploads/
        var normalized = relativeUrl.Replace('\\', '/').Trim();
        if (!normalized.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Skip delete outside uploads: {Url}", relativeUrl);
            return Task.CompletedTask;
        }

        var relativePath = normalized.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_env.WebRootPath, relativePath);

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete file {Path}", fullPath);
            }
        }

        return Task.CompletedTask;
    }

    private static void Validate(IFormFile file, string[] allowedExtensions, long maxBytes)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Không có file được gửi.");

        if (file.Length > maxBytes)
            throw new InvalidOperationException($"Kích thước file vượt quá {maxBytes / (1024 * 1024)}MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Định dạng không được hỗ trợ. Cho phép: {string.Join(", ", allowedExtensions)}");
    }

    private string EnsureUploadDir(string subFolder)
    {
        var safe = subFolder.Replace("..", "").Trim().Trim('/', '\\');
        var dir = Path.Combine(_env.WebRootPath, "uploads", safe);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        return dir;
    }

    private static string ToRelativeUrl(string subFolder, string filename)
    {
        var safe = subFolder.Replace("..", "").Trim().Trim('/', '\\').Replace('\\', '/');
        return $"/uploads/{safe}/{filename}";
    }
}
