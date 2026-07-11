namespace StudyMate.Services.Interfaces;

/// <summary>
/// Lưu / xóa file upload local dưới wwwroot/uploads.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lưu file thô. Trả về relative URL (vd /uploads/certificates/xxx.pdf).
    /// Throws <see cref="InvalidOperationException"/> nếu sai định dạng hoặc quá size.
    /// </summary>
    Task<string> SaveAsync(
        IFormFile file,
        string subFolder,
        string[] allowedExtensions,
        long maxBytes);

    /// <summary>
    /// Lưu ảnh và resize (cover/crop center) về width x height (px).
    /// </summary>
    Task<string> SaveImageResizedAsync(
        IFormFile file,
        string subFolder,
        string[] allowedExtensions,
        long maxBytes,
        int width,
        int height);

    /// <summary>
    /// Xóa file local nếu relativeUrl trỏ vào /uploads/ và file tồn tại.
    /// </summary>
    Task DeleteIfExistsAsync(string? relativeUrl);
}
