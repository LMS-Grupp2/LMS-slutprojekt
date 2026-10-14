using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CourseUserCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseUsers",
                table: "CourseUsers");

            migrationBuilder.DropIndex(
                name: "IX_CourseUsers_CourseId_UserId",
                table: "CourseUsers");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CourseUsers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseUsers",
                table: "CourseUsers",
                columns: new[] { "CourseId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseUsers",
                table: "CourseUsers");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "CourseUsers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseUsers",
                table: "CourseUsers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CourseUsers_CourseId_UserId",
                table: "CourseUsers",
                columns: new[] { "CourseId", "UserId" },
                unique: true);
        }
    }
}
