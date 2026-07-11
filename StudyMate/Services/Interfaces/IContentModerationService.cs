namespace StudyMate.Services.Interfaces;

/// <summary>
/// Kiểm duyệt nội dung ảnh/chứng chỉ (AI). M2-T12 stub.
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Trả về ghi chú kiểm duyệt (ghi vào AiVerifyNote). Không throw nếu service lỗi.
    /// </summary>
    Task<string> ReviewCertificateAsync(string? fileUrl, string title);
}
