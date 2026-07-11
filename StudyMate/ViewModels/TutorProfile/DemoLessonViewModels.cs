using System.ComponentModel.DataAnnotations;

namespace StudyMate.ViewModels.TutorProfile;

public class DemoLessonListViewModel
{
    public List<DemoLessonItemViewModel> Items { get; set; } = new();
    public DemoLessonFormViewModel Form { get; set; } = new();
}

public class DemoLessonItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? VideoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DemoLessonFormViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [StringLength(200)]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Display(Name = "Video URL")]
    [StringLength(500)]
    public string? VideoUrl { get; set; }
}
