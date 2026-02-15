using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations.ScheduleDb
{
    /// <inheritdoc />
    public partial class TableRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SelectedElectiveLessonEntries",
                table: "SelectedElectiveLessonEntries");

            migrationBuilder.RenameTable(
                name: "SelectedElectiveLessonEntries",
                newName: "SelectedElectiveLessons");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SelectedElectiveLessons",
                table: "SelectedElectiveLessons",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SelectedElectiveLessons",
                table: "SelectedElectiveLessons");

            migrationBuilder.RenameTable(
                name: "SelectedElectiveLessons",
                newName: "SelectedElectiveLessonEntries");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SelectedElectiveLessonEntries",
                table: "SelectedElectiveLessonEntries",
                column: "Id");
        }
    }
}
