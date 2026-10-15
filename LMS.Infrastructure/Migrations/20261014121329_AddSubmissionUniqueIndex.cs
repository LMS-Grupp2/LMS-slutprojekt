using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_ActivityId",
                table: "Submissions");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ActivityId_StudentId",
                table: "Submissions",
                columns: new[] { "ActivityId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Submissions_ActivityId_StudentId",
                table: "Submissions");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ActivityId",
                table: "Submissions",
                column: "ActivityId");
        }
    }
}
