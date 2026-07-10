using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StudyMate.ViewModels.Account;

/// <summary>
/// Form upload chứng chỉ (Tutor).
/// </summary>
public class UploadCertificateViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên chứng chỉ")]
    [StringLength(200, ErrorMessage = "Tên chứng chỉ tối đa 200 ký tự")]
    [Display(Name = "Tên chứng chỉ")]
    public string Title { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Đơn vị cấp tối đa 200 ký tự")]
    [Display(Name = "Đơn vị cấp")]
    public string? IssuedBy { get; set; }

    [Display(Name = "Ngày cấp")]
    [DataType(DataType.Date)]
    public DateOnly? IssuedDate { get; set; }

    [Display(Name = "Loại")]
    public string? CertType { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn file chứng chỉ")]
    [Display(Name = "File đính kèm")]
    public IFormFile? File { get; set; }
}

/// <summary>
/// Item hiển thị trong danh sách chứng chỉ.
/// </summary>
public class CertificateItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? IssuedBy { get; set; }
    public DateOnly? IssuedDate { get; set; }
    public string? FileUrl { get; set; }
    public string? CertType { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Trang Certificates: danh sách + form upload.
/// </summary>
public class CertificatesPageViewModel
{
    public List<CertificateItemViewModel> Certificates { get; set; } = new();
    public UploadCertificateViewModel Upload { get; set; } = new();
}
