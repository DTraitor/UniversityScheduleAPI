using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataAccess.Migrations.ScheduleDb
{
    /// <inheritdoc />
    public partial class HugeRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LessonSourceType",
                table: "UserLessons",
                newName: "SelectedLessonSourceType");

            migrationBuilder.CreateTable(
                name: "SelectedLessonEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    EntryId = table.Column<int>(type: "integer", nullable: false),
                    EntryName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    WeekNumber = table.Column<bool>(type: "boolean", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedLessonEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SelectedLessonSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    SubGroupNumber = table.Column<int>(type: "integer", nullable: false),
                    LessonSourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedLessonSources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserModifications_Id",
                table: "UserModifications",
                column: "Id");

            migrationBuilder.Sql(@"
                INSERT INTO public.""SelectedLessonSources"" (
                    ""UserId"",
                    ""SourceId"",
                    ""SubGroupNumber"",
                    ""LessonSourceType"",
                    ""SourceName""
                )
                SELECT
                    u.""Id"" AS ""UserId"",
                    ls.""Id"" AS ""SourceId"",
                    -1 AS ""SubGroupNumber"",
                    ls.""SourceType"" AS ""LessonSourceType"",
                    ls.""Name"" AS ""SourceName""
                FROM public.""Users"" u
                JOIN public.""Groups"" g
                    ON g.""Id"" = u.""GroupId""
                JOIN public.""LessonSources"" ls
                    ON ls.""Name"" = g.""GroupName"";
                ");

            migrationBuilder.DropTable(
                name: "ElectedLessons");

            migrationBuilder.DropTable(
                name: "ElectiveLessonDays");

            migrationBuilder.DropTable(
                name: "ElectiveLessonModifications");

            migrationBuilder.DropTable(
                name: "ElectiveLessons");

            migrationBuilder.DropTable(
                name: "GroupLessonModifications");

            migrationBuilder.DropTable(
                name: "GroupLessons");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_UserModifications_ToProcess",
                table: "UserModifications");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ToProcess",
                table: "UserModifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SelectedLessonEntries");

            migrationBuilder.DropTable(
                name: "SelectedLessonSources");

            migrationBuilder.DropIndex(
                name: "IX_UserModifications_Id",
                table: "UserModifications");

            migrationBuilder.RenameColumn(
                name: "SelectedLessonSourceType",
                table: "UserLessons",
                newName: "LessonSourceType");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ToProcess",
                table: "UserModifications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ElectedLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectiveLessonDayId = table.Column<int>(type: "integer", nullable: false),
                    ElectiveLessonId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectedLessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectiveLessonDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayId = table.Column<int>(type: "integer", nullable: false),
                    HourId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectiveLessonDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectiveLessonModifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectiveDayId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectiveLessonModifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ElectiveLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ElectiveLessonDayId = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Teacher = table.Column<string[]>(type: "text[]", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElectiveLessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupLessonModifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupLessonModifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Teacher = table.Column<string[]>(type: "text[]", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    Week = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupLessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FacultyName = table.Column<string>(type: "text", nullable: false),
                    GroupName = table.Column<string>(type: "text", nullable: false),
                    SchedulePageHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserModifications_ToProcess",
                table: "UserModifications",
                column: "ToProcess");

            migrationBuilder.CreateIndex(
                name: "IX_ElectedLessons_UserId",
                table: "ElectedLessons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveLessons_ElectiveLessonDayId",
                table: "ElectiveLessons",
                column: "ElectiveLessonDayId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLessons_GroupId",
                table: "GroupLessons",
                column: "GroupId");
        }
    }
}
