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

    // Module 1-T9 / chuẩn bị M2 — chứng chỉ Tutor (FK UserId tạm thời; M2-T2 → TutorProfileId)
    public DbSet<TutorCertificate> TutorCertificates { get; set; }

    // Module 2
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TutorProfile> TutorProfiles { get; set; }
    public DbSet<TutorSubject> TutorSubjects { get; set; }

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

        // ── TutorCertificate ─────────────────────────────────────────────────
        builder.Entity<TutorCertificate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.IssuedBy).HasMaxLength(200);
            e.Property(x => x.FileUrl).HasMaxLength(500);
            e.Property(x => x.CertType).HasMaxLength(50);
            e.Property(x => x.IsVerified).HasDefaultValue(false);
            e.HasOne<ApplicationUser>()
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.UserId).HasDatabaseName("IX_TutorCertificates_UserId");
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

        // ── Index tối ưu query phổ biến ──────────────────────────────────────
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("IX_AspNetUsers_Email");
    }
}
