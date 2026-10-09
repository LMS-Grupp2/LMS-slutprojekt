using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Activities");

            migrationBuilder.AddColumn<Guid>(
                name: "ActivityTypeId",
                table: "Activities",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ActivityTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ActivityTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("47c20d6b-daf9-4180-a01c-092f5337af72"), "Any other scheduled activity, such as a kickoff or a meeting.", "Other" },
                    { new Guid("59fb72bf-fd37-48dc-8391-5ee265c0072c"), "A teacher-led session where new material is presented.", "Lecture" },
                    { new Guid("8998f0ff-48b5-4245-943f-3579252e4d18"), "A practice session where you apply what you've learned.", "Exercise" },
                    { new Guid("9acbd254-d038-44b9-ba1f-eeea65500778"), "Self-paced online material, such as a video course, done on your own time.", "E-Learning" },
                    { new Guid("c10e45b9-fcfe-468c-bc7e-fe29b4fb62eb"), "A task to complete and hand in for review by the teacher.", "Assignment" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_ActivityTypeId",
                table: "Activities",
                column: "ActivityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTypes_Name",
                table: "ActivityTypes",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_ActivityTypes_ActivityTypeId",
                table: "Activities",
                column: "ActivityTypeId",
                principalTable: "ActivityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_ActivityTypes_ActivityTypeId",
                table: "Activities");

            migrationBuilder.DropTable(
                name: "ActivityTypes");

            migrationBuilder.DropIndex(
                name: "IX_Activities_ActivityTypeId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ActivityTypeId",
                table: "Activities");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Activities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
