-- ============================================================
--  Seed Users for Development/Testing
--  Database: StudyMate (SQL Server)
--
--  Passwords (ASP.NET Identity v3 hash):
--    All users: Test@1234
--
--  ⚠️  DEVELOPMENT ONLY — DO NOT run on production
-- ============================================================

USE StudyMate;
GO

-- ============================================================
-- Lấy RoleId từ bảng AspNetRoles
-- ============================================================
DECLARE @AdminRoleId    NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE NormalizedName = 'ADMIN');
DECLARE @TutorRoleId    NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE NormalizedName = 'TUTOR');
DECLARE @StudentRoleId  NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE NormalizedName = 'STUDENT');

-- ============================================================
-- User IDs
-- ============================================================
DECLARE @AdminId    NVARCHAR(450) = 'seed-admin-0001-0000-000000000001';
DECLARE @TutorId    NVARCHAR(450) = 'seed-tutor-0001-0000-000000000002';
DECLARE @StudentId  NVARCHAR(450) = 'seed-student-001-0000-000000000003';

-- ============================================================
-- Password hash cho "Test@1234"
-- Generated bằng ASP.NET Identity PasswordHasher v3
-- ============================================================
DECLARE @PasswordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAELgBioMJMivPmFjBmExDqp2ajX3jZ/5h/EsR5P0RRhNoY6c1Zb7p0ZjDh2m3xVdv9Q==';

-- ============================================================
-- Xóa seed users cũ nếu có (idempotent)
-- ============================================================
DELETE FROM AspNetUserRoles WHERE UserId IN (@AdminId, @TutorId, @StudentId);
DELETE FROM AspNetUsers     WHERE Id    IN (@AdminId, @TutorId, @StudentId);

-- ============================================================
-- Insert Users
-- ============================================================

-- Admin
INSERT INTO AspNetUsers (
    Id, FullName, IsActive, IsEmailVerified, CreatedAt,
    UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
) VALUES (
    @AdminId, N'Admin StudyMate', 1, 1, GETDATE(),
    'admin@studymate.dev', 'ADMIN@STUDYMATE.DEV',
    'admin@studymate.dev', 'ADMIN@STUDYMATE.DEV',
    1, @PasswordHash, NEWID(), NEWID(),
    0, 0, 1, 0
);

-- Tutor
INSERT INTO AspNetUsers (
    Id, FullName, IsActive, IsEmailVerified, CreatedAt,
    UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
) VALUES (
    @TutorId, N'Nguyễn Văn Tutor', 1, 1, GETDATE(),
    'tutor@studymate.dev', 'TUTOR@STUDYMATE.DEV',
    'tutor@studymate.dev', 'TUTOR@STUDYMATE.DEV',
    1, @PasswordHash, NEWID(), NEWID(),
    0, 0, 1, 0
);

-- Student
INSERT INTO AspNetUsers (
    Id, FullName, IsActive, IsEmailVerified, CreatedAt,
    UserName, NormalizedUserName, Email, NormalizedEmail,
    EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
    PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount
) VALUES (
    @StudentId, N'Trần Thị Student', 1, 1, GETDATE(),
    'student@studymate.dev', 'STUDENT@STUDYMATE.DEV',
    'student@studymate.dev', 'STUDENT@STUDYMATE.DEV',
    1, @PasswordHash, NEWID(), NEWID(),
    0, 0, 1, 0
);

-- ============================================================
-- Gán Roles
-- ============================================================
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@AdminId,   @AdminRoleId);
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@TutorId,   @TutorRoleId);
INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES (@StudentId, @StudentRoleId);

-- ============================================================
-- Verify
-- ============================================================
SELECT u.Email, u.FullName, r.Name AS Role
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r      ON ur.RoleId = r.Id
WHERE u.Id IN (@AdminId, @TutorId, @StudentId);

PRINT 'Seed users inserted successfully.';
GO
