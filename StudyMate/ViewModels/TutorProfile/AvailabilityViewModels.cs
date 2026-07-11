using System.ComponentModel.DataAnnotations;

namespace StudyMate.ViewModels.TutorProfile;

public class AvailabilityPageViewModel
{
    public List<AvailabilityItemViewModel> Items { get; set; } = new();
    public AvailabilityFormViewModel Form { get; set; } = new();
}

public class AvailabilityItemViewModel
{
    public int Id { get; set; }
    public int DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public string DayName => DayOfWeek switch
    {
        0 => "Chủ nhật",
        1 => "Thứ 2",
        2 => "Thứ 3",
        3 => "Thứ 4",
        4 => "Thứ 5",
        5 => "Thứ 6",
        6 => "Thứ 7",
        _ => "?"
    };
}

public class AvailabilityFormViewModel
{
    [Display(Name = "Ngày")]
    [Range(0, 6)]
    public int DayOfWeek { get; set; } = 1;

    [Display(Name = "Bắt đầu")]
    [DataType(DataType.Time)]
    public TimeOnly StartTime { get; set; } = new(18, 0);

    [Display(Name = "Kết thúc")]
    [DataType(DataType.Time)]
    public TimeOnly EndTime { get; set; } = new(20, 0);
}
