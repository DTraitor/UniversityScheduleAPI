using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations.ScheduleDb
{
    /// <inheritdoc />
    public partial class cchange_indexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserModifications_Id_ToProcess",
                table: "UserModifications");

            migrationBuilder.DropIndex(
                name: "IX_UserLessons_Id_UserId",
                table: "UserLessons");

            migrationBuilder.DropIndex(
                name: "IX_GroupLessons_Id_GroupId",
                table: "GroupLessons");

            migrationBuilder.DropIndex(
                name: "IX_ElectiveLessons_Id_ElectiveLessonDayId",
                table: "ElectiveLessons");

            migrationBuilder.DropIndex(
                name: "IX_ElectedLessons_Id_UserId",
                table: "ElectedLessons");

            migrationBuilder.CreateIndex(
                name: "IX_UserModifications_ToProcess",
                table: "UserModifications",
                column: "ToProcess");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_UserId",
                table: "UserLessons",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonOccurrences_LessonId",
                table: "UserLessonOccurrences",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonOccurrences_UserId",
                table: "UserLessonOccurrences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLessons_GroupId",
                table: "GroupLessons",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveLessons_ElectiveLessonDayId",
                table: "ElectiveLessons",
                column: "ElectiveLessonDayId");

            migrationBuilder.CreateIndex(
                name: "IX_ElectedLessons_UserId",
                table: "ElectedLessons",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserModifications_ToProcess",
                table: "UserModifications");

            migrationBuilder.DropIndex(
                name: "IX_UserLessons_UserId",
                table: "UserLessons");

            migrationBuilder.DropIndex(
                name: "IX_UserLessonOccurrences_LessonId",
                table: "UserLessonOccurrences");

            migrationBuilder.DropIndex(
                name: "IX_UserLessonOccurrences_UserId",
                table: "UserLessonOccurrences");

            migrationBuilder.DropIndex(
                name: "IX_GroupLessons_GroupId",
                table: "GroupLessons");

            migrationBuilder.DropIndex(
                name: "IX_ElectiveLessons_ElectiveLessonDayId",
                table: "ElectiveLessons");

            migrationBuilder.DropIndex(
                name: "IX_ElectedLessons_UserId",
                table: "ElectedLessons");

            migrationBuilder.CreateIndex(
                name: "IX_UserModifications_Id_ToProcess",
                table: "UserModifications",
                columns: new[] { "Id", "ToProcess" });

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_Id_UserId",
                table: "UserLessons",
                columns: new[] { "Id", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroupLessons_Id_GroupId",
                table: "GroupLessons",
                columns: new[] { "Id", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectiveLessons_Id_ElectiveLessonDayId",
                table: "ElectiveLessons",
                columns: new[] { "Id", "ElectiveLessonDayId" });

            migrationBuilder.CreateIndex(
                name: "IX_ElectedLessons_Id_UserId",
                table: "ElectedLessons",
                columns: new[] { "Id", "UserId" });
        }
    }
}
