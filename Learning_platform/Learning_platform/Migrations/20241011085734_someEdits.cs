using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Learning_platform.Migrations
{
    /// <inheritdoc />
    public partial class someEdits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "6d078a4e-4951-460b-90b9-a8410e932477");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "Lessons",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "StudentLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentLessons_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentLessons_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "8be650b8-64fc-43f2-9729-3f5cdb3c1bdc");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "1db33115-f262-4af5-a83e-3ca93020fa89", "97baf32f-a29a-4ec7-b1cc-2fdf5c7c6969", "User", "user" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "b219b702-92dc-421a-a69b-11fa9be29dae", "AQAAAAIAAYagAAAAEKbGXB8XdYpvl9do0Fluuj6aiKDtNeVNvDOZiaXoBeoMPdD4QI0EW0joIvasUJUk/A==" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentLessons_ApplicationUserId",
                table: "StudentLessons",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentLessons_LessonId",
                table: "StudentLessons",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentLessons");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1db33115-f262-4af5-a83e-3ca93020fa89");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "Lessons");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2",
                column: "ConcurrencyStamp",
                value: "2804a6ba-e0d7-44ee-99df-d6db7601cd9e");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "6d078a4e-4951-460b-90b9-a8410e932477", "c177aea6-4c1f-45dd-870a-2f6326394975", "User", "user" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "9ede9808-569c-4784-b950-f6118cc9caa5", "AQAAAAIAAYagAAAAEIygri5V5gFXY6KrTW5ClzfHSMQyksEgDBaIt6iAdOXccw5RXPzG5WgEWgn39QuUVQ==" });
        }
    }
}
