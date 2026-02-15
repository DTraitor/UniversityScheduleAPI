using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations.ScheduleDb
{
    /// <inheritdoc />
    public partial class indexs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_OccurrencesCalculatedTill",
                table: "UserLessons",
                column: "OccurrencesCalculatedTill");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessons_RepeatType",
                table: "UserLessons",
                column: "RepeatType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserLessons_OccurrencesCalculatedTill",
                table: "UserLessons");

            migrationBuilder.DropIndex(
                name: "IX_UserLessons_RepeatType",
                table: "UserLessons");
        }
    }
}
