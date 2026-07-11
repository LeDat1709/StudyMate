using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyMate.Migrations
{
    /// <inheritdoc />
    public partial class AddTutorProfileRelatedAndMigrateCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Certificates: UserId → TutorProfileId (data-safe) ──────────
            migrationBuilder.DropForeignKey(
                name: "FK_TutorCertificates_AspNetUsers_UserId",
                table: "TutorCertificates");

            migrationBuilder.DropIndex(
                name: "IX_TutorCertificates_UserId",
                table: "TutorCertificates");

            migrationBuilder.AddColumn<int>(
                name: "TutorProfileId",
                table: "TutorCertificates",
                type: "int",
                nullable: true);

            // Map certs of users who already have a TutorProfile
            migrationBuilder.Sql("""
                UPDATE c
                SET c.TutorProfileId = p.Id
                FROM TutorCertificates c
                INNER JOIN TutorProfiles p ON p.UserId = c.UserId;
                """);

            // Orphan certs (no profile yet) cannot keep NOT NULL FK — remove
            migrationBuilder.Sql("""
                DELETE FROM TutorCertificates WHERE TutorProfileId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TutorCertificates");

            migrationBuilder.AlterColumn<int>(
                name: "TutorProfileId",
                table: "TutorCertificates",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            // ── 2. DemoLessons ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "DemoLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TutorProfileId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DemoLessons_TutorProfiles_TutorProfileId",
                        column: x => x.TutorProfileId,
                        principalTable: "TutorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── 3. TutorAvailabilities ───────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "TutorAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TutorProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TutorAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TutorAvailabilities_TutorProfiles_TutorProfileId",
                        column: x => x.TutorProfileId,
                        principalTable: "TutorProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TutorCertificates_TutorProfileId",
                table: "TutorCertificates",
                column: "TutorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoLessons_TutorProfileId",
                table: "DemoLessons",
                column: "TutorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TutorAvailabilities_TutorProfileId",
                table: "TutorAvailabilities",
                column: "TutorProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorCertificates_TutorProfiles_TutorProfileId",
                table: "TutorCertificates",
                column: "TutorProfileId",
                principalTable: "TutorProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TutorCertificates_TutorProfiles_TutorProfileId",
                table: "TutorCertificates");

            migrationBuilder.DropTable(
                name: "DemoLessons");

            migrationBuilder.DropTable(
                name: "TutorAvailabilities");

            migrationBuilder.DropIndex(
                name: "IX_TutorCertificates_TutorProfileId",
                table: "TutorCertificates");

            migrationBuilder.DropColumn(
                name: "TutorProfileId",
                table: "TutorCertificates");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "TutorCertificates",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TutorCertificates_UserId",
                table: "TutorCertificates",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TutorCertificates_AspNetUsers_UserId",
                table: "TutorCertificates",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
