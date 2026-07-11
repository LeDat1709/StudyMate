using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyMate.Models;

namespace StudyMate.Data;

/// <summary>
/// DbContext chính của StudyMate, kế thừa IdentityDbContext để tích hợp ASP.NET Identity.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ── Các DbSet sẽ được thêm dần theo từng module ──────────────────────────
    // Module 1
    public DbSet<OtpCode> OtpCodes { get; set; }

    // Module 2
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TutorProfile> TutorProfiles { get; set; }
    public DbSet<TutorSubject> TutorSubjects { get; set; }
    public DbSet<TutorCertificate> TutorCertificates { get; set; }
    public DbSet<TutorAvailability> TutorAvailabilities { get; set; }
    public DbSet<DemoLesson> DemoLessons { get; set; }

    // Module 3
    public DbSet<JobPosting> JobPostings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── OtpCode ──────────────────────────────────────────────────────────
        builder.Entity<OtpCode>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(10).IsRequired();
            e.Property(x => x.Purpose).HasMaxLength(50).IsRequired();
            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── TutorCertificate (FK TutorProfile) ────────────────────────────────
        builder.Entity<TutorCertificate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.IssuedBy).HasMaxLength(200);
            e.Property(x => x.FileUrl).HasMaxLength(500);
            e.Property(x => x.CertType).HasMaxLength(50);
            e.Property(x => x.IsVerified).HasDefaultValue(false);
            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.Certificates)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TutorProfileId).HasDatabaseName("IX_TutorCertificates_TutorProfileId");
        });

        // ── TutorAvailability ────────────────────────────────────────────────
        builder.Entity<TutorAvailability>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.Availabilities)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TutorProfileId).HasDatabaseName("IX_TutorAvailabilities_TutorProfileId");
        });

        // ── DemoLesson ───────────────────────────────────────────────────────
        builder.Entity<DemoLesson>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.VideoUrl).HasMaxLength(500);
            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.DemoLessons)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.TutorProfileId).HasDatabaseName("IX_DemoLessons_TutorProfileId");
        });

        // ── Subject ──────────────────────────────────────────────────────────
        builder.Entity<Subject>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Category).HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("IX_Subjects_Name");
        });

        // ── TutorProfile ─────────────────────────────────────────────────────
        builder.Entity<TutorProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            e.Property(x => x.Headline).HasMaxLength(200);
            e.Property(x => x.VideoIntroUrl).HasMaxLength(500);
            e.Property(x => x.EducationLevel).HasMaxLength(100);
            e.Property(x => x.TeachingMode).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.HourlyRate).HasPrecision(10, 2);
            e.Property(x => x.AverageRating).HasPrecision(3, 2).HasDefaultValue(0m);
            e.Property(x => x.IsVerified).HasDefaultValue(false);
            e.Property(x => x.IsAvailable).HasDefaultValue(true);
            e.Property(x => x.TotalReviews).HasDefaultValue(0);

            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId)
             .IsUnique()
             .HasDatabaseName("IX_TutorProfiles_UserId");

            e.HasIndex(x => new { x.IsAvailable, x.IsVerified })
             .HasDatabaseName("IX_TutorProfiles_IsAvailable");
        });

        // ── TutorSubject (composite key) ─────────────────────────────────────
        builder.Entity<TutorSubject>(e =>
        {
            e.HasKey(x => new { x.TutorProfileId, x.SubjectId });

            e.HasOne(x => x.TutorProfile)
             .WithMany(p => p.TutorSubjects)
             .HasForeignKey(x => x.TutorProfileId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Subject)
             .WithMany(s => s.TutorSubjects)
             .HasForeignKey(x => x.SubjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── JobPosting (M3) ──────────────────────────────────────────────────
        builder.Entity<JobPosting>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.StudentId).HasMaxLength(450).IsRequired();
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.DesiredLevel).HasMaxLength(100);
            e.Property(x => x.TeachingMode).HasMaxLength(20);
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");
            e.Property(x => x.BudgetMin).HasPrecision(10, 2);
            e.Property(x => x.BudgetMax).HasPrecision(10, 2);

            e.HasOne(x => x.Student)
             .WithMany()
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Subject)
             .WithMany()
             .HasForeignKey(x => x.SubjectId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.StudentId).HasDatabaseName("IX_JobPostings_StudentId");
            e.HasIndex(x => new { x.Status, x.Deadline }).HasDatabaseName("IX_JobPostings_Status");
        });

        // ── Index tối ưu query phổ biến ──────────────────────────────────────
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("IX_AspNetUsers_Email");
    }
}
