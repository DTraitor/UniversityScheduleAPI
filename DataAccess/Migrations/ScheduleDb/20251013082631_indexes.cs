using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations.ScheduleDb
{
    /// <inheritdoc />
    public partial class indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserModifications_Id_ToProcess",
                table: "UserModifications",
                columns: new[] { "Id", "ToProcess" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_Id_UserId",
                table: "UserLessons",
                columns: new[] { "Id", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonOccurrences_LessonId_UserId",
                table: "UserLessonOccurrences",
                columns: new[] { "LessonId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAlerts_Id",
                table: "UserAlerts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLessons_Id_GroupId",
                table: "GroupLessons",
                columns: new[] { "Id", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveLessons_Id_ElectiveLessonDayId",
                table: "ElectiveLessons",
                columns: new[] { "Id", "ElectiveLessonDayId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Id",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserModifications_Id_ToProcess",
                table: "UserModifications");

            migrationBuilder.DropIndex(
                name: "IX_UserLessons_Id_UserId",
                table: "UserLessons");

            migrationBuilder.DropIndex(
                name: "IX_UserLessonOccurrences_LessonId_UserId",
                table: "UserLessonOccurrences");

            migrationBuilder.DropIndex(
                name: "IX_UserAlerts_Id",
                table: "UserAlerts");

            migrationBuilder.DropIndex(
                name: "IX_GroupLessons_Id_GroupId",
                table: "GroupLessons");

            migrationBuilder.DropIndex(
                name: "IX_ElectiveLessons_Id_ElectiveLessonDayId",
                table: "ElectiveLessons");
        }
    }
}
