using Microsoft.EntityFrameworkCore;
using StudyMate.Models;

namespace StudyMate.Data;

/// <summary>
/// Seed dữ liệu nền (Subjects, …). Idempotent — chỉ insert khi bảng trống / thiếu.
/// </summary>
public static class SeedData
{
    private static readonly (string Name, string Category)[] DefaultSubjects =
    [
        ("Toán", "THPT"),
        ("Vật lý", "THPT"),
        ("Hóa học", "THPT"),
        ("Tiếng Anh", "Ngoại ngữ"),
        ("IELTS", "Ngoại ngữ"),
        ("TOEIC", "Ngoại ngữ"),
        ("Lập trình C#", "Công nghệ"),
        ("Lập trình Python", "Công nghệ"),
        ("Cơ sở dữ liệu", "Công nghệ"),
        ("Văn học", "THPT"),
        ("Lịch sử", "THPT"),
        ("Địa lý", "THPT"),
    ];

    /// <summary>
    /// Seed danh sách môn học nếu chưa có (theo Name).
    /// </summary>
    public static async Task SeedSubjectsAsync(ApplicationDbContext db)
    {
        var existing = await db.Subjects
            .Select(s => s.Name)
            .ToListAsync();

        var existingSet = new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
        var toAdd = DefaultSubjects
            .Where(s => !existingSet.Contains(s.Name))
            .Select(s => new Subject { Name = s.Name, Category = s.Category })
            .ToList();

        if (toAdd.Count == 0)
            return;

        db.Subjects.AddRange(toAdd);
        await db.SaveChangesAsync();
    }
}
